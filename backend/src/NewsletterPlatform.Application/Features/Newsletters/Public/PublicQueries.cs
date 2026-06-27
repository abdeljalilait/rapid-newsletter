using MediatR;
using NewsletterPlatform.Application.Interfaces;

namespace NewsletterPlatform.Application.Features.Newsletters.Public;

public sealed record GetPublicWorkspaceQuery(string WorkspaceSlug) : IRequest<PublicWorkspaceDto?>;
public sealed record GetPublicPlansQuery(string WorkspaceSlug) : IRequest<IReadOnlyCollection<PlanDto>?>;
public sealed record GetPublicPostsQuery(string WorkspaceSlug) : IRequest<IReadOnlyCollection<PostDto>?>;
public sealed record GetPublicPostQuery(string WorkspaceSlug, Guid PostId) : IRequest<PublicPostDetailDto?>;

internal sealed class GetPublicWorkspaceHandler : IRequestHandler<GetPublicWorkspaceQuery, PublicWorkspaceDto?>
{
    private readonly IPublicWorkspaceReader _reader;
    public GetPublicWorkspaceHandler(IPublicWorkspaceReader reader) => _reader = reader;
    public async Task<PublicWorkspaceDto?> Handle(GetPublicWorkspaceQuery request, CancellationToken ct) =>
        await _reader.GetPublicWorkspaceAsync(request.WorkspaceSlug, ct);
}

internal sealed class GetPublicPlansHandler : IRequestHandler<GetPublicPlansQuery, IReadOnlyCollection<PlanDto>?>
{
    private readonly IPublicWorkspaceReader _reader;
    public GetPublicPlansHandler(IPublicWorkspaceReader reader) => _reader = reader;
    public async Task<IReadOnlyCollection<PlanDto>?> Handle(GetPublicPlansQuery request, CancellationToken ct) =>
        await _reader.GetPublicPlansAsync(request.WorkspaceSlug, ct);
}

internal sealed class GetPublicPostsHandler : IRequestHandler<GetPublicPostsQuery, IReadOnlyCollection<PostDto>?>
{
    private readonly IPublicWorkspaceReader _reader;
    public GetPublicPostsHandler(IPublicWorkspaceReader reader) => _reader = reader;
    public async Task<IReadOnlyCollection<PostDto>?> Handle(GetPublicPostsQuery request, CancellationToken ct) =>
        await _reader.GetPublicPostsAsync(request.WorkspaceSlug, ct);
}

internal sealed class GetPublicPostHandler : IRequestHandler<GetPublicPostQuery, PublicPostDetailDto?>
{
    private readonly IPublicWorkspaceReader _reader;
    public GetPublicPostHandler(IPublicWorkspaceReader reader) => _reader = reader;
    public async Task<PublicPostDetailDto?> Handle(GetPublicPostQuery request, CancellationToken ct) =>
        await _reader.GetPublicPostAsync(request.WorkspaceSlug, request.PostId, ct);
}
