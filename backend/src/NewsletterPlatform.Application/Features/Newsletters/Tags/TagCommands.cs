using MediatR;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters.Tags;

public sealed record GetTagsQuery(Guid WorkspaceId) : IRequest<IReadOnlyCollection<TagDto>>;
public sealed record CreateTagCommand(Guid WorkspaceId, NameRequest Request) : IRequest<TagDto>;

internal sealed class GetTagsHandler : IRequestHandler<GetTagsQuery, IReadOnlyCollection<TagDto>>
{
    private readonly ITagRepository _tags;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;

    public GetTagsHandler(ITagRepository tags, IWorkspaceAuthorization auth, ICurrentUserService current)
    {
        _tags = tags;
        _auth = auth;
        _current = current;
    }

    public async Task<IReadOnlyCollection<TagDto>> Handle(GetTagsQuery request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to view tags.");

        return await _tags.GetTagsAsync(request.WorkspaceId, ct);
    }
}

internal sealed class CreateTagHandler : IRequestHandler<CreateTagCommand, TagDto>
{
    private readonly ITagRepository _tags;
    private readonly IWorkspaceAuthorization _auth;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public CreateTagHandler(ITagRepository tags, IWorkspaceAuthorization auth, ICurrentUserService current, IUnitOfWork uow)
    {
        _tags = tags;
        _auth = auth;
        _current = current;
        _uow = uow;
    }

    public async Task<TagDto> Handle(CreateTagCommand request, CancellationToken ct)
    {
        if (_current.UserId is null)
            throw new Exceptions.UnauthorizedException("Authentication required.");
        if (!await _auth.HasWorkspaceRoleAsync(_current.UserId.Value, request.WorkspaceId, WorkspaceRole.Admin, ct))
            throw new Exceptions.ForbiddenException("You do not have permission to manage tags.");

        var result = await _tags.CreateTagAsync(request.WorkspaceId, request.Request, ct);
        await _uow.SaveChangesAsync(ct);
        return result;
    }
}
