using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Plans;

public sealed record GetPlansQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<PlanDto>>;
public sealed record CreatePlanCommand(Guid WorkspaceId, PlanRequest Request) : IRequest<PlanDto>;

internal sealed class GetPlansHandler : IRequestHandler<GetPlansQuery, IReadOnlyCollection<PlanDto>>
{
    private readonly IPlanRepository _plans;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetPlansHandler(IPlanRepository plans, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _plans = plans;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<PlanDto>> Handle(GetPlansQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view plans.");

        return await _plans.GetPlansAsync(request.WorkspaceId, ct);
    }
}

internal sealed class CreatePlanHandler : IRequestHandler<CreatePlanCommand, PlanDto>
{
    private readonly IPlanRepository _plans;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreatePlanHandler(IPlanRepository plans, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _plans = plans;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<PlanDto> Handle(CreatePlanCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage plans.");

        var result = await _plans.CreatePlanAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}
