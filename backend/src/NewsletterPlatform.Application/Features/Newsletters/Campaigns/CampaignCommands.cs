using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Campaigns;

public sealed record GetCampaignsQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<CampaignDto>>;
public sealed record CreateCampaignCommand(Guid WorkspaceId, CampaignRequest Request) : IRequest<CampaignDto>;
public sealed record EstimateCampaignCapacityQuery(Guid WorkspaceId, Guid CampaignId, CampaignLaunchRequest Request) : IRequest<CampaignCapacityDto?>;
public sealed record LaunchCampaignCommand(Guid WorkspaceId, Guid CampaignId, CampaignLaunchRequest Request) : IRequest<LaunchCampaignResult?>;
public sealed record ClaimCampaignDispatchBatchCommand(Guid WorkspaceId, Guid CampaignId, CampaignDispatchClaimRequest Request) : IRequest<CampaignDispatchBatchDto?>;
public sealed record SetCampaignStatusCommand(Guid WorkspaceId, Guid CampaignId, CampaignStatus Status) : IRequest<CampaignDto?>;

internal sealed class GetCampaignsHandler : IRequestHandler<GetCampaignsQuery, IReadOnlyCollection<CampaignDto>>
{
    private readonly ICampaignRepository _campaigns;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetCampaignsHandler(ICampaignRepository campaigns, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _campaigns = campaigns;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<CampaignDto>> Handle(GetCampaignsQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view campaigns.");

        return await _campaigns.GetCampaignsAsync(request.WorkspaceId, ct);
    }
}

internal sealed class CreateCampaignHandler : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
    private readonly ICampaignRepository _campaigns;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreateCampaignHandler(ICampaignRepository campaigns, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _campaigns = campaigns;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<CampaignDto> Handle(CreateCampaignCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage campaigns.");

        var result = await _campaigns.CreateCampaignAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}

internal sealed class EstimateCampaignCapacityHandler : IRequestHandler<EstimateCampaignCapacityQuery, CampaignCapacityDto?>
{
    private readonly ICampaignRepository _campaigns;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public EstimateCampaignCapacityHandler(ICampaignRepository campaigns, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _campaigns = campaigns;
        _auth = auth;
        _current = current;
    }

    public async Task<CampaignCapacityDto?> Handle(EstimateCampaignCapacityQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage campaigns.");

        return await _campaigns.EstimateCampaignCapacityAsync(request.WorkspaceId, request.CampaignId, request.Request, ct);
    }
}

internal sealed class LaunchCampaignHandler : IRequestHandler<LaunchCampaignCommand, LaunchCampaignResult?>
{
    private readonly ICampaignRepository _campaigns;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public LaunchCampaignHandler(ICampaignRepository campaigns, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _campaigns = campaigns;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<LaunchCampaignResult?> Handle(LaunchCampaignCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage campaigns.");

        var result = await _campaigns.LaunchCampaignAsync(request.WorkspaceId, request.CampaignId, request.Request, ct);
        if (result is not null && result.Error is null)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}

internal sealed class SetCampaignStatusHandler : IRequestHandler<SetCampaignStatusCommand, CampaignDto?>
{
    private readonly ICampaignRepository _campaigns;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public SetCampaignStatusHandler(ICampaignRepository campaigns, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _campaigns = campaigns;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<CampaignDto?> Handle(SetCampaignStatusCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage campaigns.");

        var result = await _campaigns.SetCampaignStatusAsync(request.WorkspaceId, request.CampaignId, request.Status, ct);
        if (result is not null)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}

internal sealed class ClaimCampaignDispatchBatchHandler : IRequestHandler<ClaimCampaignDispatchBatchCommand, CampaignDispatchBatchDto?>
{
    private readonly ICampaignRepository _campaigns;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public ClaimCampaignDispatchBatchHandler(ICampaignRepository campaigns, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _campaigns = campaigns;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<CampaignDispatchBatchDto?> Handle(ClaimCampaignDispatchBatchCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage campaigns.");

        var result = await _campaigns.ClaimCampaignDispatchBatchAsync(request.WorkspaceId, request.CampaignId, request.Request, ct);
        if (result is not null && result.ClaimedCount > 0)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}
