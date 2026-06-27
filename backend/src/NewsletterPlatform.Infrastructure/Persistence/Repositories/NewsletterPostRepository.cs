using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Common;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository
{
    public async Task<IReadOnlyCollection<PostDto>> GetPostsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.Posts.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => PostDto.From(x))
            .ToArrayAsync(ct);
    }

    public async Task<PublicPostDetailDto?> GetPublicPostAsync(string workspaceSlug, Guid postId, CancellationToken ct = default)
    {
        var workspaceId = await _db.Workspaces.AsNoTracking()
            .Where(x => x.Slug == workspaceSlug)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);
        if (workspaceId is null) return null;

        return await _db.Posts.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId
                && x.Id == postId
                && x.PublishOnWebsite
                && x.Status == PostStatus.Published
                && x.Audience == PostAudience.Public)
            .Select(x => PublicPostDetailDto.From(x))
            .FirstOrDefaultAsync(ct);
    }

    public Task<PostDto> CreatePostAsync(Guid workspaceId, PostRequest request, CancellationToken ct = default)
    {
        var post = new Post
        {
            WorkspaceId = workspaceId,
            Title = request.Title.Trim(),
            Slug = Slug.Normalize(request.Slug ?? request.Title),
            Subtitle = request.Subtitle,
            PreviewText = request.PreviewText,
            CoverImageUrl = request.CoverImageUrl,
            EditorContentJson = request.EditorContentJson,
            RenderedHtml = request.RenderedHtml,
            PlainText = request.PlainText,
            Audience = request.Audience,
            Status = request.Status,
            PublishOnWebsite = request.PublishOnWebsite,
            SendByEmail = request.SendByEmail,
            ScheduledAt = request.ScheduledAt,
            PublishedAt = request.Status == PostStatus.Published ? DateTime.UtcNow : null
        };

        _db.Posts.Add(post);
        return Task.FromResult(PostDto.From(post));
    }

    public async Task<OverviewAnalyticsDto> GetOverviewAnalyticsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        var grouped = await _db.Subscribers
            .Where(x => x.WorkspaceId == workspaceId)
            .GroupBy(x => x.AccessLevel)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToArrayAsync(ct);

        var total = grouped.Sum(x => x.Count);
        var free = grouped.Where(x => x.Key == SubscriberAccessLevel.Free).Sum(x => x.Count);
        var paid = grouped.Where(x => x.Key == SubscriberAccessLevel.Paid).Sum(x => x.Count);
        var activeSubscriptions = await _db.ReaderSubscriptions.CountAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.Status == ReaderSubscriptionStatus.Active, ct);

        var now = DateTime.UtcNow;
        var sentThisMonth = await _db.EmailEvents.CountAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.EventType == EmailEventType.Sent &&
            x.OccurredAt.Month == now.Month &&
            x.OccurredAt.Year == now.Year, ct);

        return new OverviewAnalyticsDto(total, free, paid, activeSubscriptions, sentThisMonth);
    }
}
