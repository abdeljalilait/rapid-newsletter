using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository :
    IWorkspaceAuthorization,
    IPublicWorkspaceReader,
    ISubscriberRepository,
    ITagRepository,
    IListRepository,
    IPlanRepository,
    IPaymentRepository,
    IProviderAccountRepository,
    IPostRepository,
    ICampaignRepository,
    IAnalyticsRepository
{
    private readonly NewsletterPlatformDbContext _db;
    private readonly ISecretProtector _protector;

    public NewsletterRepository(NewsletterPlatformDbContext db, ISecretProtector protector)
    {
        _db = db;
        _protector = protector;
    }

    public async Task<bool> HasWorkspaceRoleAsync(Guid userId, Guid workspaceId, WorkspaceRole requiredRole, CancellationToken ct = default)
    {
        var role = await _db.WorkspaceMembers.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId && x.UserId == userId)
            .Select(x => (WorkspaceRole?)x.Role)
            .FirstOrDefaultAsync(ct);

        return role.HasValue && HasRole(role.Value, requiredRole);
    }

    private async Task<Guid?> GetWorkspaceIdBySlugAsync(string slug, CancellationToken ct) =>
        await _db.Workspaces.AsNoTracking()
            .Where(x => x.Slug == slug)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(ct);

    private async Task<int> CountQueuedRecipientsAsync(Guid workspaceId, Guid campaignId, CancellationToken ct) =>
        await _db.CampaignRecipients.CountAsync(x =>
            x.WorkspaceId == workspaceId &&
            x.CampaignId == campaignId &&
            x.Status == CampaignRecipientStatus.Queued, ct);

    private static bool HasRole(WorkspaceRole actual, WorkspaceRole required)
    {
        if (required == WorkspaceRole.Viewer) return true;
        if (required == WorkspaceRole.Editor) return actual is WorkspaceRole.Owner or WorkspaceRole.Admin or WorkspaceRole.Editor;
        if (required == WorkspaceRole.Admin) return actual is WorkspaceRole.Owner or WorkspaceRole.Admin;
        return actual == WorkspaceRole.Owner;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
    private static bool IsValidEmail(string email) => !string.IsNullOrWhiteSpace(email) && email.Contains('@', StringComparison.Ordinal);

    private static AudienceFilterDto DeserializeFilter(string json) =>
        JsonSerializer.Deserialize<AudienceFilterDto>(json) ?? new AudienceFilterDto();

    private IQueryable<Subscriber> BuildAudienceQuery(Guid workspaceId, AudienceFilterDto filter)
    {
        var query = _db.Subscribers.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .Where(x => x.Status == SubscriberStatus.Active)
            .Where(x => !_db.Suppressions.Any(s => s.WorkspaceId == workspaceId && s.Email == x.Email));

        if (filter.AccessLevel.HasValue)
            query = query.Where(x => x.AccessLevel == filter.AccessLevel.Value);
        if (filter.Status.HasValue)
            query = query.Where(x => x.Status == filter.Status.Value);
        if (filter.JoinedFrom.HasValue)
            query = query.Where(x => x.SubscribedAt >= filter.JoinedFrom.Value);
        if (filter.JoinedTo.HasValue)
            query = query.Where(x => x.SubscribedAt <= filter.JoinedTo.Value);
        if (filter.TagIds is { Count: > 0 })
            query = query.Where(x => _db.SubscriberTags.Any(t =>
                t.WorkspaceId == workspaceId &&
                t.SubscriberId == x.Id &&
                filter.TagIds.Contains(t.TagId)));
        if (filter.ListIds is { Count: > 0 })
            query = query.Where(x => _db.AudienceListMembers.Any(m =>
                m.WorkspaceId == workspaceId &&
                m.SubscriberId == x.Id &&
                filter.ListIds.Contains(m.AudienceListId)));

        return query.OrderBy(x => x.Email);
    }

    private async Task<CapacityEstimate> EstimateCapacityAsync(Guid workspaceId, IReadOnlyCollection<SelectedProviderAccountRequest> selected, CancellationToken ct)
    {
        var ids = selected.Where(x => x.Enabled).Select(x => x.ProviderAccountId).ToArray();
        var accounts = await _db.EmailProviderAccounts.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId &&
                ids.Contains(x.Id) &&
                x.Enabled &&
                !x.IsDeleted &&
                x.HealthStatus == ProviderHealthStatus.Active)
            .ToArrayAsync(ct);

        var rows = new List<ProviderCapacityDto>();
        foreach (var account in accounts)
        {
            var request = selected.First(x => x.ProviderAccountId == account.Id);
            var caps = new[] { account.DailyLimit ?? int.MaxValue, account.MonthlyLimit ?? int.MaxValue, request.MaximumEmails ?? int.MaxValue };
            var remaining = caps.Min();
            if (remaining == int.MaxValue) remaining = 1_000_000;
            rows.Add(new ProviderCapacityDto(account.Id, account.AccountName, remaining, request.RatePerMinute));
        }

        return new CapacityEstimate(rows.Sum(x => x.RemainingCapacity), rows.Sum(x => x.RatePerMinute), rows);
    }

    private sealed record CapacityEstimate(int TotalCapacity, int CombinedRatePerMinute, IReadOnlyCollection<ProviderCapacityDto> Accounts);
}
