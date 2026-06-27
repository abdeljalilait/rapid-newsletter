using NewsletterPlatform.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace NewsletterPlatform.Application.Features.Workspaces.Commands;

public sealed record UpdateWorkspaceCommand(
    Guid WorkspaceId,
    string Name,
    string DefaultSenderName,
    string DefaultSenderEmail,
    string? Description,
    string? LogoUrl,
    string Timezone,
    string DefaultCurrency) : IRequest<WorkspaceDto>;

internal sealed class UpdateWorkspaceHandler : IRequestHandler<UpdateWorkspaceCommand, WorkspaceDto>
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _current;
    private readonly IAuditLogger _audit;

    public UpdateWorkspaceHandler(IWorkspaceRepository workspaces, IUnitOfWork uow, ICurrentUserService current, IAuditLogger audit)
    {
        _workspaces = workspaces; _uow = uow; _current = current; _audit = audit;
    }

    public async Task<WorkspaceDto> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");

        var ws = await _workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken)
            ?? throw new Exceptions.NotFoundException("Workspace", request.WorkspaceId);

        var member = ws.Members.FirstOrDefault(m => m.UserId == _current.UserId.Value);
        if (member is null || member.Role > Domain.Enums.WorkspaceRole.Admin)
            throw new Exceptions.ForbiddenException("You do not have permission to update this workspace.");

        ws.Name = request.Name.Trim();
        ws.DefaultSenderName = request.DefaultSenderName.Trim();
        ws.DefaultSenderEmail = request.DefaultSenderEmail.Trim().ToLowerInvariant();
        ws.Description = request.Description;
        ws.LogoUrl = request.LogoUrl;
        ws.Timezone = string.IsNullOrWhiteSpace(request.Timezone) ? ws.Timezone : request.Timezone;
        ws.DefaultCurrency = string.IsNullOrWhiteSpace(request.DefaultCurrency) ? ws.DefaultCurrency : request.DefaultCurrency.ToUpperInvariant();
        ws.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(ws.Id, _current.UserId, "workspace.updated", "Workspace", ws.Id, null, _current.IpAddress, cancellationToken);

        return new WorkspaceDto(
            ws.Id, ws.Name, ws.Slug, ws.LogoUrl, ws.Description,
            ws.DefaultSenderName, ws.DefaultSenderEmail, ws.Timezone,
            ws.DefaultCurrency, ws.Status.ToString(), member.Role.ToString(), ws.CreatedAt);
    }
}

public sealed class UpdateWorkspaceCommandValidator : AbstractValidator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DefaultSenderName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.DefaultSenderEmail).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Timezone).MaximumLength(64);
        RuleFor(x => x.DefaultCurrency).Length(3);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
