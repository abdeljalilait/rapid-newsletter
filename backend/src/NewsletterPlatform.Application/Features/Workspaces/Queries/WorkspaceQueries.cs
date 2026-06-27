using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using MediatR;

namespace NewsletterPlatform.Application.Features.Workspaces.Queries;

public sealed record GetMyWorkspacesQuery : IRequest<IReadOnlyCollection<WorkspaceDto>>;

internal sealed class GetMyWorkspacesHandler : IRequestHandler<GetMyWorkspacesQuery, IReadOnlyCollection<WorkspaceDto>>
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly ICurrentUserService _current;

    public GetMyWorkspacesHandler(IWorkspaceRepository workspaces, ICurrentUserService current)
    {
        _workspaces = workspaces; _current = current;
    }

    public async Task<IReadOnlyCollection<WorkspaceDto>> Handle(GetMyWorkspacesQuery request, CancellationToken cancellationToken)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");

        var workspaces = await _workspaces.GetWorkspacesForUserAsync(_current.UserId.Value, cancellationToken);
        return workspaces.Select(Map).ToArray();
    }

    private static WorkspaceDto Map(Workspace ws)
    {
        var role = ws.Members.FirstOrDefault(m => m.UserId == ws.OwnerId)?.Role ?? WorkspaceRole.Viewer;
        return new WorkspaceDto(
            ws.Id, ws.Name, ws.Slug, ws.LogoUrl, ws.Description,
            ws.DefaultSenderName, ws.DefaultSenderEmail, ws.Timezone,
            ws.DefaultCurrency, ws.Status.ToString(), role.ToString(), ws.CreatedAt);
    }
}

public sealed record GetWorkspaceByIdQuery(Guid Id) : IRequest<WorkspaceDto>;

internal sealed class GetWorkspaceByIdHandler : IRequestHandler<GetWorkspaceByIdQuery, WorkspaceDto>
{
    private readonly IWorkspaceRepository _workspaces;
    private readonly ICurrentUserService _current;

    public GetWorkspaceByIdHandler(IWorkspaceRepository workspaces, ICurrentUserService current)
    {
        _workspaces = workspaces; _current = current;
    }

    public async Task<WorkspaceDto> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");

        var ws = await _workspaces.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new Exceptions.NotFoundException("Workspace", request.Id);

        var member = ws.Members.FirstOrDefault(m => m.UserId == _current.UserId.Value)
            ?? throw new Exceptions.ForbiddenException("You do not have access to this workspace.");

        return new WorkspaceDto(
            ws.Id, ws.Name, ws.Slug, ws.LogoUrl, ws.Description,
            ws.DefaultSenderName, ws.DefaultSenderEmail, ws.Timezone,
            ws.DefaultCurrency, ws.Status.ToString(), member.Role.ToString(), ws.CreatedAt);
    }
}