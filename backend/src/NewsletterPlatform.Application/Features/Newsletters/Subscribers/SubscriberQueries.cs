using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Subscribers;

public sealed record GetSubscribersQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<SubscriberDto>>;

internal sealed class GetSubscribersHandler : IRequestHandler<GetSubscribersQuery, IReadOnlyCollection<SubscriberDto>>
{
    private readonly ISubscriberRepository _subscribers;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetSubscribersHandler(ISubscriberRepository subscribers, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _subscribers = subscribers;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<SubscriberDto>> Handle(GetSubscribersQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Viewer, ct))
            throw new Exceptions.ForbiddenException("You do not have access to this workspace.");

        return await _subscribers.GetSubscribersAsync(request.WorkspaceId, ct);
    }
}
