using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository
{
    public async Task<PublicWorkspaceDto?> GetPublicWorkspaceAsync(string workspaceSlug, CancellationToken ct = default)
    {
        return await _db.Workspaces.AsNoTracking()
            .Where(x => x.Slug == workspaceSlug)
            .Select(x => new PublicWorkspaceDto(x.Id, x.Name, x.Slug, x.LogoUrl, x.Description))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyCollection<PlanDto>?> GetPublicPlansAsync(string workspaceSlug, CancellationToken ct = default)
    {
        var workspaceId = await GetWorkspaceIdBySlugAsync(workspaceSlug, ct);
        if (workspaceId is null) return null;

        return await _db.SubscriptionPlans.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => PlanDto.From(x))
            .ToArrayAsync(ct);
    }

    public async Task<IReadOnlyCollection<PostDto>?> GetPublicPostsAsync(string workspaceSlug, CancellationToken ct = default)
    {
        var workspaceId = await GetWorkspaceIdBySlugAsync(workspaceSlug, ct);
        if (workspaceId is null) return null;

        return await _db.Posts.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId &&
                x.PublishOnWebsite &&
                x.Status == PostStatus.Published &&
                x.Audience == PostAudience.Public)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => PostDto.From(x))
            .ToArrayAsync(ct);
    }

    public async Task<CreatedDto<SubscriberDto>?> SubscribePublicAsync(string workspaceSlug, PublicSubscribeRequest request, CancellationToken ct = default)
    {
        var workspaceId = await GetWorkspaceIdBySlugAsync(workspaceSlug, ct);
        if (workspaceId is null) return null;

        var email = NormalizeEmail(request.Email);
        var existing = await _db.Subscribers.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Email == email, ct);
        if (existing is not null)
        {
            if (existing.Status == SubscriberStatus.Unsubscribed)
            {
                existing.Status = SubscriberStatus.Active;
                await RemoveGlobalUnsubscribeAsync(workspaceId.Value, existing.Id, email, ct);
            }
            existing.FirstName = request.FirstName?.Trim();
            existing.LastName = request.LastName?.Trim();
            existing.ConsentSource = "public_subscribe_form";
            existing.ConsentAt = DateTime.UtcNow;
            return new CreatedDto<SubscriberDto>(SubscriberDto.From(existing), false);
        }

        var subscriber = new Subscriber
        {
            WorkspaceId = workspaceId.Value,
            Email = email,
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            Status = SubscriberStatus.Active,
            AccessLevel = SubscriberAccessLevel.Free,
            ConsentSource = "public_subscribe_form",
            ConsentAt = DateTime.UtcNow
        };

        _db.Subscribers.Add(subscriber);
        return new CreatedDto<SubscriberDto>(SubscriberDto.From(subscriber), true);
    }

    public async Task<PublicUnsubscribeDto?> UnsubscribePublicAsync(string workspaceSlug, PublicUnsubscribeRequest request, CancellationToken ct = default)
    {
        var workspaceId = await GetWorkspaceIdBySlugAsync(workspaceSlug, ct);
        if (workspaceId is null) return null;

        var email = NormalizeEmail(request.Email);
        var subscriber = await _db.Subscribers
            .FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Email == email, ct);
        if (subscriber is null) return null;

        subscriber.Status = SubscriberStatus.Unsubscribed;
        subscriber.LastEngagementAt = DateTime.UtcNow;

        var hasPreference = await _db.UnsubscribePreferences.AnyAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.SubscriberId == subscriber.Id &&
            x.AudienceListId == null &&
            x.UnsubscribedFromAll, ct);

        if (!hasPreference)
        {
            _db.UnsubscribePreferences.Add(new UnsubscribePreference
            {
                WorkspaceId = workspaceId.Value,
                SubscriberId = subscriber.Id,
                UnsubscribedFromAll = true
            });
        }

        var hasSuppression = await _db.Suppressions.AnyAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.Email == email &&
            x.Reason == SuppressionReason.GlobalUnsubscribe, ct);

        if (!hasSuppression)
        {
            _db.Suppressions.Add(new Suppression
            {
                WorkspaceId = workspaceId.Value,
                SubscriberId = subscriber.Id,
                Email = email,
                Reason = SuppressionReason.GlobalUnsubscribe,
                Notes = "Reader unsubscribed from all marketing."
            });
        }

        return new PublicUnsubscribeDto(subscriber.Id, subscriber.Email, subscriber.Status, true);
    }

    private async Task RemoveGlobalUnsubscribeAsync(Guid workspaceId, Guid subscriberId, string email, CancellationToken ct)
    {
        var preferences = await _db.UnsubscribePreferences
            .Where(x => x.WorkspaceId == workspaceId &&
                x.SubscriberId == subscriberId &&
                x.AudienceListId == null &&
                x.UnsubscribedFromAll)
            .ToArrayAsync(ct);
        _db.UnsubscribePreferences.RemoveRange(preferences);

        var suppressions = await _db.Suppressions
            .Where(x => x.WorkspaceId == workspaceId &&
                x.Email == email &&
                x.Reason == SuppressionReason.GlobalUnsubscribe)
            .ToArrayAsync(ct);
        _db.Suppressions.RemoveRange(suppressions);
    }

    public async Task<IReadOnlyCollection<SubscriberDto>> GetSubscribersAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.Subscribers.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => SubscriberDto.From(x))
            .ToArrayAsync(ct);
    }

    public async Task<IReadOnlyCollection<string>> GetSubscriberEmailsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.Subscribers
            .Where(x => x.WorkspaceId == workspaceId)
            .Select(x => x.Email)
            .ToArrayAsync(ct);
    }

    public async Task<CreatedDto<SubscriberDto>?> CreateSubscriberAsync(Guid workspaceId, UpsertSubscriberRequest request, CancellationToken ct = default)
    {
        var email = NormalizeEmail(request.Email);
        if (await _db.Subscribers.AnyAsync(x => x.WorkspaceId == workspaceId && x.Email == email, ct))
            return null;

        var subscriber = new Subscriber
        {
            WorkspaceId = workspaceId,
            Email = email,
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            Status = request.Status,
            AccessLevel = request.AccessLevel,
            ConsentSource = request.ConsentSource,
            ConsentAt = request.ConsentAt ?? DateTime.UtcNow
        };

        _db.Subscribers.Add(subscriber);
        return new CreatedDto<SubscriberDto>(SubscriberDto.From(subscriber), true);
    }

    public async Task<ImportSummaryDto> ImportSubscribersAsync(Guid workspaceId, ImportSubscribersRequest request, CancellationToken ct = default)
    {
        var imported = 0;
        var duplicates = 0;
        var invalid = 0;
        var existingEmails = await _db.Subscribers
            .Where(x => x.WorkspaceId == workspaceId)
            .Select(x => x.Email)
            .ToHashSetAsync(ct);

        foreach (var row in request.Rows)
        {
            if (!IsValidEmail(row.Email))
            {
                invalid++;
                continue;
            }

            var email = NormalizeEmail(row.Email);
            if (existingEmails.Contains(email))
            {
                duplicates++;
                continue;
            }

            _db.Subscribers.Add(new Subscriber
            {
                WorkspaceId = workspaceId,
                Email = email,
                FirstName = row.FirstName?.Trim(),
                LastName = row.LastName?.Trim(),
                Status = SubscriberStatus.Active,
                AccessLevel = row.AccessLevel,
                ConsentSource = "admin_import",
                ConsentAt = DateTime.UtcNow
            });
            imported++;
            existingEmails.Add(email);
        }

        return new ImportSummaryDto(request.Rows.Count, imported, duplicates, invalid, 0);
    }
}
