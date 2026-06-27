using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Common;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using FluentValidation;
using MediatR;

namespace NewsletterPlatform.Application.Features.Workspaces.Commands;

public sealed record CreateWorkspaceCommand(
    string Name,
    string? Slug,
    string? Description,
    string DefaultSenderName,
    string DefaultSenderEmail,
    string? LogoUrl,
    string Timezone,
    string DefaultCurrency) : IRequest<WorkspaceDto>;

internal sealed class CreateWorkspaceHandler : IRequestHandler<CreateWorkspaceCommand, WorkspaceDto>
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _current;
    private readonly IAuditLogger _audit;

    public CreateWorkspaceHandler(IWorkspaceRepository workspaces, IUserRepository users, IUnitOfWork uow, ICurrentUserService current, IAuditLogger audit)
    {
        _workspaces = workspaces; _users = users; _uow = uow; _current = current; _audit = audit;
    }

    public async Task<WorkspaceDto> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");

        var ownerId = _current.UserId.Value;
        var slug = Slug.Normalize(request.Slug ?? request.Name);
        if (!Slug.IsValid(slug))
            throw new Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Slug", "Slug must be 1-63 chars, lowercase letters, numbers and hyphens."),
            });

        if (await _workspaces.SlugExistsAsync(slug, cancellationToken))
            throw new Exceptions.ConflictException("This workspace slug is already taken.");

        var user = await _users.GetByIdAsync(ownerId, cancellationToken)
            ?? throw new Exceptions.UnauthorizedException("User not found.");

        var workspace = new Workspace
        {
            OwnerId = ownerId,
            Name = request.Name.Trim(),
            Slug = slug,
            DefaultSenderName = request.DefaultSenderName.Trim(),
            DefaultSenderEmail = request.DefaultSenderEmail.Trim().ToLowerInvariant(),
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            Timezone = string.IsNullOrWhiteSpace(request.Timezone) ? "UTC" : request.Timezone,
            DefaultCurrency = string.IsNullOrWhiteSpace(request.DefaultCurrency) ? "USD" : request.DefaultCurrency.ToUpperInvariant(),
            Status = WorkspaceStatus.Active
        };
        workspace.Members.Add(new WorkspaceMember
        {
            WorkspaceId = workspace.Id,
            UserId = ownerId,
            Role = WorkspaceRole.Owner,
            JoinedAt = DateTime.UtcNow
        });

        await _workspaces.AddAsync(workspace, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(workspace.Id, ownerId, "workspace.created", "Workspace", workspace.Id, null, _current.IpAddress, cancellationToken);

        return new WorkspaceDto(
            workspace.Id, workspace.Name, workspace.Slug, workspace.LogoUrl, workspace.Description,
            workspace.DefaultSenderName, workspace.DefaultSenderEmail, workspace.Timezone,
            workspace.DefaultCurrency, workspace.Status.ToString(), nameof(WorkspaceRole.Owner), workspace.CreatedAt);
    }
}

public sealed class CreateWorkspaceCommandValidator : FluentValidation.AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DefaultSenderName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DefaultSenderEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Timezone).MaximumLength(64);
        RuleFor(x => x.DefaultCurrency).Length(3);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
