using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Subscribers;

public sealed record CreateSubscriberCommand(Guid WorkspaceId, UpsertSubscriberRequest Request) : IRequest<CreatedDto<SubscriberDto>?>;
public sealed record ImportSubscribersCommand(Guid WorkspaceId, ImportSubscribersRequest Request) : IRequest<ImportSummaryDto>;

internal sealed class CreateSubscriberHandler : IRequestHandler<CreateSubscriberCommand, CreatedDto<SubscriberDto>?>
{
    private readonly ISubscriberRepository _subscribers;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreateSubscriberHandler(ISubscriberRepository subscribers, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _subscribers = subscribers;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<CreatedDto<SubscriberDto>?> Handle(CreateSubscriberCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage subscribers.");

        var result = await _subscribers.CreateSubscriberAsync(request.WorkspaceId, request.Request, ct);
        if (result is not null)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}

internal sealed class ImportSubscribersHandler : IRequestHandler<ImportSubscribersCommand, ImportSummaryDto>
{
    private readonly ISubscriberRepository _subscribers;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public ImportSubscribersHandler(ISubscriberRepository subscribers, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _subscribers = subscribers;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<ImportSummaryDto> Handle(ImportSubscribersCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage subscribers.");

        var result = await _subscribers.ImportSubscribersAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}
