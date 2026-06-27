using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Analytics;

public sealed record GetOverviewAnalyticsQuery(Guid WorkspaceId) : IRequest<OverviewAnalyticsDto>;

internal sealed class GetOverviewAnalyticsHandler : IRequestHandler<GetOverviewAnalyticsQuery, OverviewAnalyticsDto>
{
    private readonly IAnalyticsRepository _analytics;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetOverviewAnalyticsHandler(IAnalyticsRepository analytics, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _analytics = analytics;
        _auth = auth;
        _current = current;
    }

    public async Task<OverviewAnalyticsDto> Handle(GetOverviewAnalyticsQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Viewer, ct))
            throw new Exceptions.ForbiddenException("You do not have access to this workspace.");

        return await _analytics.GetOverviewAnalyticsAsync(request.WorkspaceId, ct);
    }
}
