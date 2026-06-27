using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Lists;

public sealed record GetListsQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<ListDto>>;
public sealed record CreateListCommand(Guid WorkspaceId, ListRequest Request) : IRequest<ListDto>;
public sealed record AddListMemberCommand(Guid WorkspaceId, Guid ListId, Guid SubscriberId) : IRequest<bool>;

internal sealed class GetListsHandler : IRequestHandler<GetListsQuery, IReadOnlyCollection<ListDto>>
{
    private readonly IListRepository _lists;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetListsHandler(IListRepository lists, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _lists = lists;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<ListDto>> Handle(GetListsQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view lists.");

        return await _lists.GetListsAsync(request.WorkspaceId, ct);
    }
}

internal sealed class CreateListHandler : IRequestHandler<CreateListCommand, ListDto>
{
    private readonly IListRepository _lists;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreateListHandler(IListRepository lists, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _lists = lists;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<ListDto> Handle(CreateListCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage lists.");

        var result = await _lists.CreateListAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}

internal sealed class AddListMemberHandler : IRequestHandler<AddListMemberCommand, bool>
{
    private readonly IListRepository _lists;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public AddListMemberHandler(IListRepository lists, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _lists = lists;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<bool> Handle(AddListMemberCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage lists.");

        var result = await _lists.AddListMemberAsync(request.WorkspaceId, request.ListId, request.SubscriberId, ct);
        if (result)
            await _uow.SaveChangesAsync(ct);
        return result;
    }
}
