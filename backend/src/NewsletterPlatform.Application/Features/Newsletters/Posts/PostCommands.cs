using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Posts;

public sealed record GetPostsQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<PostDto>>;
public sealed record CreatePostCommand(Guid WorkspaceId, PostRequest Request) : IRequest<PostDto>;

internal sealed class GetPostsHandler : IRequestHandler<GetPostsQuery, IReadOnlyCollection<PostDto>>
{
    private readonly IPostRepository _posts;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetPostsHandler(IPostRepository posts, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _posts = posts;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<PostDto>> Handle(GetPostsQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view posts.");

        return await _posts.GetPostsAsync(request.WorkspaceId, ct);
    }
}

internal sealed class CreatePostHandler : IRequestHandler<CreatePostCommand, PostDto>
{
    private readonly IPostRepository _posts;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreatePostHandler(IPostRepository posts, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _posts = posts;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Editor, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage posts.");

        var result = await _posts.CreatePostAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}
