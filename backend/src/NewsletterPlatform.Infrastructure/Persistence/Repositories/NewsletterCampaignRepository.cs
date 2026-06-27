using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.Persistence.Repositories;

public sealed partial class NewsletterRepository
{
    public async Task<IReadOnlyCollection<CampaignDto>> GetCampaignsAsync(Guid workspaceId, CancellationToken ct = default)
    {
        return await _db.Campaigns.AsNoTracking()
            .Where(x => x.WorkspaceId == workspaceId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => CampaignDto.From(x))
            .ToArrayAsync(ct);
    }

    public Task<CampaignDto> CreateCampaignAsync(Guid workspaceId, CampaignRequest request, CancellationToken ct = default)
    {
        var campaign = new Campaign
        {
            WorkspaceId = workspaceId,
            PostId = request.PostId,
            Name = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            PreviewText = request.PreviewText,
            FromName = request.FromName.Trim(),
            FromEmail = NormalizeEmail(request.FromEmail),
            ReplyTo = request.ReplyTo,
            BodyHtml = request.BodyHtml,
            PlainText = request.PlainText,
            AudienceFilterJson = System.Text.Json.JsonSerializer.Serialize(request.AudienceFilter),
            ScheduledAt = request.ScheduledAt,
            Status = CampaignStatus.Draft,
            AllowPartialCampaign = request.AllowPartialCampaign
        };

        _db.Campaigns.Add(campaign);
        return Task.FromResult(CampaignDto.From(campaign));
    }

    public async Task<CampaignCapacityDto?> EstimateCampaignCapacityAsync(Guid workspaceId, Guid campaignId, CampaignLaunchRequest request, CancellationToken ct = default)
    {
        var campaign = await _db.Campaigns.AsNoTracking().FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == campaignId, ct);
        if (campaign is null) return null;

        var recipients = await BuildAudienceQuery(workspaceId, DeserializeFilter(campaign.AudienceFilterJson)).CountAsync(ct);
        var capacity = await EstimateCapacityAsync(workspaceId, request.ProviderAccounts, ct);
        return new CampaignCapacityDto(recipients, capacity.TotalCapacity, Math.Max(0, recipients - capacity.TotalCapacity), capacity.CombinedRatePerMinute, capacity.Accounts);
    }

    public async Task<LaunchCampaignResult?> LaunchCampaignAsync(Guid workspaceId, Guid campaignId, CampaignLaunchRequest request, CancellationToken ct = default)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == campaignId, ct);
        if (campaign is null) return null;
        if (campaign.Status != CampaignStatus.Draft)
            return new LaunchCampaignResult(null, "Only draft campaigns can be launched.");

        var recipients = await BuildAudienceQuery(workspaceId, DeserializeFilter(campaign.AudienceFilterJson)).ToArrayAsync(ct);
        var capacity = await EstimateCapacityAsync(workspaceId, request.ProviderAccounts, ct);
        var allowPartial = request.AllowPartialCampaign || campaign.AllowPartialCampaign;
        if (capacity.TotalCapacity < recipients.Length && !allowPartial)
            return new LaunchCampaignResult(null, "Selected provider capacity is insufficient.", recipients.Length, capacity.TotalCapacity);

        foreach (var selected in request.ProviderAccounts)
            AddCampaignProviderAccount(workspaceId, campaign.Id, selected);

        foreach (var subscriber in allowPartial ? recipients.Take(capacity.TotalCapacity) : recipients)
            AddCampaignRecipient(workspaceId, campaign.Id, subscriber);

        campaign.RecipientCount = allowPartial ? Math.Min(recipients.Length, capacity.TotalCapacity) : recipients.Length;
        campaign.Status = campaign.ScheduledAt.HasValue && campaign.ScheduledAt.Value > DateTime.UtcNow
            ? CampaignStatus.Scheduled
            : CampaignStatus.Preparing;
        campaign.AllowPartialCampaign = allowPartial;

        return new LaunchCampaignResult(CampaignDto.From(campaign), null);
    }

    public async Task<CampaignDispatchBatchDto?> ClaimCampaignDispatchBatchAsync(Guid workspaceId, Guid campaignId, CampaignDispatchClaimRequest request, CancellationToken ct = default)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == campaignId, ct);
        if (campaign is null) return null;
        if (!CanClaimCampaign(campaign))
            return new CampaignDispatchBatchDto(campaignId, 0, await CountQueuedRecipientsAsync(workspaceId, campaignId, ct), []);

        var selectedAccounts = await GetEligibleCampaignAccounts(workspaceId, campaignId, ct);
        if (selectedAccounts.Length == 0)
            return new CampaignDispatchBatchDto(campaignId, 0, await CountQueuedRecipientsAsync(workspaceId, campaignId, ct), []);

        var maxItems = Math.Clamp(request.MaxItems, 1, 500);
        var now = DateTime.UtcNow;
        var providerIds = selectedAccounts.Select(x => x.ProviderAccountId).ToArray();
        var providers = await GetActiveProviders(workspaceId, providerIds, ct);
        var recentAssignments = await GetRecentAssignments(workspaceId, campaignId, providerIds, now.AddMinutes(-1), ct);
        var queued = await GetQueuedRecipients(workspaceId, campaignId, maxItems, ct);
        var claimed = ClaimRecipients(selectedAccounts, providers, recentAssignments, queued, now);

        if (claimed.Count > 0 && campaign.Status != CampaignStatus.Sending)
        {
            campaign.Status = CampaignStatus.Sending;
            campaign.UpdatedAt = now;
        }

        var remainingQueued = await CountQueuedRecipientsAsync(workspaceId, campaignId, ct) - claimed.Count;
        return new CampaignDispatchBatchDto(campaignId, claimed.Count, Math.Max(0, remainingQueued), claimed);
    }

    public async Task<CampaignDto?> SetCampaignStatusAsync(Guid workspaceId, Guid campaignId, CampaignStatus status, CancellationToken ct = default)
    {
        var campaign = await _db.Campaigns.FirstOrDefaultAsync(x => x.WorkspaceId == workspaceId && x.Id == campaignId, ct);
        if (campaign is null) return null;

        campaign.Status = status;
        return CampaignDto.From(campaign);
    }

    private void AddCampaignProviderAccount(Guid workspaceId, Guid campaignId, SelectedProviderAccountRequest selected)
    {
        _db.CampaignProviderAccounts.Add(new CampaignProviderAccount
        {
            WorkspaceId = workspaceId,
            CampaignId = campaignId,
            ProviderAccountId = selected.ProviderAccountId,
            Priority = selected.Priority,
            RatePerMinute = selected.RatePerMinute,
            MaximumEmails = selected.MaximumEmails,
            Enabled = selected.Enabled
        });
    }

    private void AddCampaignRecipient(Guid workspaceId, Guid campaignId, Subscriber subscriber)
    {
        _db.CampaignRecipients.Add(new CampaignRecipient
        {
            WorkspaceId = workspaceId,
            CampaignId = campaignId,
            SubscriberId = subscriber.Id,
            Email = subscriber.Email,
            Status = CampaignRecipientStatus.Queued
        });
    }
}
