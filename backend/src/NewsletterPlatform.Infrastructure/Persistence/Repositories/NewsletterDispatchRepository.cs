using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository
{
    private static bool CanClaimCampaign(Campaign campaign)
    {
        if (campaign.ScheduledAt.HasValue && campaign.ScheduledAt.Value > DateTime.UtcNow)
            return false;

        return campaign.Status is not CampaignStatus.Draft
            and not CampaignStatus.Paused
            and not CampaignStatus.Completed
            and not CampaignStatus.PartiallyCompleted
            and not CampaignStatus.Cancelled
            and not CampaignStatus.Failed;
    }

    private async Task<CampaignProviderAccount[]> GetEligibleCampaignAccounts(Guid workspaceId, Guid campaignId, CancellationToken ct)
    {
        return await _db.CampaignProviderAccounts
            .Where(x => x.WorkspaceId == workspaceId && x.CampaignId == campaignId && x.Enabled)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.SentCount)
            .ThenBy(x => x.CreatedAt)
            .ToArrayAsync(ct);
    }

    private async Task<Dictionary<Guid, EmailProviderAccount>> GetActiveProviders(Guid workspaceId, Guid[] providerIds, CancellationToken ct)
    {
        return await _db.EmailProviderAccounts.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId &&
                providerIds.Contains(x.Id) &&
                x.Enabled &&
                !x.IsDeleted &&
                x.HealthStatus == ProviderHealthStatus.Active)
            .ToDictionaryAsync(x => x.Id, ct);
    }

    private async Task<Dictionary<Guid, int>> GetRecentAssignments(Guid workspaceId, Guid campaignId, Guid[] providerIds, DateTime minuteWindowStart, CancellationToken ct)
    {
        return await _db.CampaignRecipients.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId &&
                x.CampaignId == campaignId &&
                x.ProviderAccountId.HasValue &&
                providerIds.Contains(x.ProviderAccountId.Value) &&
                x.LastAttemptAt.HasValue &&
                x.LastAttemptAt >= minuteWindowStart &&
                (x.Status == CampaignRecipientStatus.Sending || x.Status == CampaignRecipientStatus.Sent))
            .GroupBy(x => x.ProviderAccountId!.Value)
            .Select(x => new { ProviderAccountId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ProviderAccountId, x => x.Count, ct);
    }

    private async Task<List<CampaignRecipient>> GetQueuedRecipients(Guid workspaceId, Guid campaignId, int maxItems, CancellationToken ct)
    {
        return await _db.CampaignRecipients
            .Where(x => x.WorkspaceId == workspaceId &&
                x.CampaignId == campaignId &&
                x.Status == CampaignRecipientStatus.Queued)
            .OrderBy(x => x.CreatedAt)
            .Take(maxItems)
            .ToListAsync(ct);
    }

    private static List<CampaignDispatchRecipientDto> ClaimRecipients(
        CampaignProviderAccount[] selectedAccounts,
        Dictionary<Guid, EmailProviderAccount> providers,
        Dictionary<Guid, int> recentAssignments,
        List<CampaignRecipient> queued,
        DateTime now)
    {
        var claimed = new List<CampaignDispatchRecipientDto>();
        foreach (var recipient in queued)
        {
            var selected = SelectAccountForRecipient(selectedAccounts, providers, recentAssignments);
            if (selected is null) break;

            var provider = providers[selected.ProviderAccountId];
            recipient.Status = CampaignRecipientStatus.Sending;
            recipient.ProviderAccountId = provider.Id;
            recipient.AttemptCount += 1;
            recipient.LastAttemptAt = now;
            recipient.UpdatedAt = now;

            selected.SentCount += 1;
            selected.UpdatedAt = now;
            recentAssignments[selected.ProviderAccountId] = recentAssignments.GetValueOrDefault(selected.ProviderAccountId) + 1;

            claimed.Add(new CampaignDispatchRecipientDto(
                recipient.Id,
                recipient.SubscriberId,
                recipient.Email,
                provider.Id,
                provider.AccountName,
                provider.Provider,
                selected.Priority));
        }

        return claimed;
    }

    private static CampaignProviderAccount? SelectAccountForRecipient(
        CampaignProviderAccount[] selectedAccounts,
        Dictionary<Guid, EmailProviderAccount> providers,
        Dictionary<Guid, int> recentAssignments)
    {
        return selectedAccounts
            .Where(x => providers.ContainsKey(x.ProviderAccountId))
            .Where(x => !x.MaximumEmails.HasValue || x.SentCount < x.MaximumEmails.Value)
            .Where(x => recentAssignments.GetValueOrDefault(x.ProviderAccountId) < Math.Max(1, x.RatePerMinute))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.SentCount)
            .ThenBy(x => recentAssignments.GetValueOrDefault(x.ProviderAccountId))
            .ThenBy(x => x.CreatedAt)
            .FirstOrDefault();
    }
}
