using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using NewsletterPlatform.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NewsletterPlatform.Infrastructure.Services;

public sealed class CampaignDispatcher : ICampaignDispatcher
{
    private readonly NewsletterPlatformDbContext _db;
    private readonly ICampaignRepository _campaigns;
    private readonly IUnitOfWork _uow;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<CampaignDispatcher> _logger;
    private readonly CampaignDispatchWorkerOptions _options;

    public CampaignDispatcher(
        NewsletterPlatformDbContext db,
        ICampaignRepository campaigns,
        IUnitOfWork uow,
        IEmailSender emailSender,
        IDateTimeProvider dateTime,
        ILogger<CampaignDispatcher> logger,
        IOptions<CampaignDispatchWorkerOptions> options)
    {
        _db = db;
        _campaigns = campaigns;
        _uow = uow;
        _emailSender = emailSender;
        _dateTime = dateTime;
        _logger = logger;
        _options = options.Value;
    }

    public async Task DispatchPendingCampaignsAsync(CancellationToken ct = default)
    {
        var now = _dateTime.UtcNow;
        var pending = await _db.Campaigns
            .AsNoTracking()
            .Where(c => c.Status == CampaignStatus.Preparing || c.Status == CampaignStatus.Sending)
            .Where(c => !c.ScheduledAt.HasValue || c.ScheduledAt <= now)
            .Where(c => _db.CampaignRecipients.Any(r =>
                r.CampaignId == c.Id &&
                r.Status == CampaignRecipientStatus.Queued))
            .Select(c => new { c.WorkspaceId, c.Id })
            .ToArrayAsync(ct);

        foreach (var campaign in pending)
        {
            try
            {
                await DispatchCampaignAsync(campaign.WorkspaceId, campaign.Id, ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch campaign {CampaignId}", campaign.Id);
            }
        }
    }

    private async Task DispatchCampaignAsync(Guid workspaceId, Guid campaignId, CancellationToken ct)
    {
        var batch = await _campaigns.ClaimCampaignDispatchBatchAsync(
            workspaceId,
            campaignId,
            new CampaignDispatchClaimRequest(_options.BatchSize),
            ct);

        if (batch is null || batch.ClaimedCount == 0)
            return;

        await _uow.SaveChangesAsync(ct);

        var campaign = await _db.Campaigns.AsNoTracking()
            .FirstAsync(c => c.Id == campaignId, ct);

        var now = _dateTime.UtcNow;
        var failures = new List<Guid>();

        foreach (var recipient in batch.Recipients)
        {
            try
            {
                await _emailSender.SendAsync(
                    recipient.Email,
                    campaign.Subject,
                    campaign.BodyHtml,
                    campaign.PlainText,
                    ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send campaign email to {Email} for campaign {CampaignId}", recipient.Email, campaignId);
                failures.Add(recipient.CampaignRecipientId);
            }
        }

        var claimedIds = batch.Recipients.Select(r => r.CampaignRecipientId).ToArray();
        var trackedRecipients = await _db.CampaignRecipients
            .Where(r => claimedIds.Contains(r.Id))
            .ToArrayAsync(ct);

        foreach (var recipient in trackedRecipients)
        {
            if (failures.Contains(recipient.Id))
            {
                recipient.Status = CampaignRecipientStatus.Failed;
                recipient.LastError = "Send failed";
            }
            else
            {
                recipient.Status = CampaignRecipientStatus.Sent;
                recipient.LastAttemptAt = now;
            }
            recipient.UpdatedAt = now;
        }

        await _uow.SaveChangesAsync(ct);

        var remainingQueued = await _db.CampaignRecipients.CountAsync(
            r => r.CampaignId == campaignId && r.Status == CampaignRecipientStatus.Queued, ct);

        if (remainingQueued == 0)
        {
            var total = await _db.CampaignRecipients.CountAsync(r => r.CampaignId == campaignId, ct);
            var failed = await _db.CampaignRecipients.CountAsync(
                r => r.CampaignId == campaignId && r.Status == CampaignRecipientStatus.Failed, ct);

            var finalStatus = failed == total
                ? CampaignStatus.Failed
                : failed > 0
                    ? CampaignStatus.PartiallyCompleted
                    : CampaignStatus.Completed;

            var campaignToUpdate = await _db.Campaigns.FirstAsync(c => c.Id == campaignId, ct);
            campaignToUpdate.Status = finalStatus;
            campaignToUpdate.UpdatedAt = now;
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Campaign {CampaignId} finished with status {Status} ({Sent}/{Total} sent)",
                campaignId,
                finalStatus,
                total - failed,
                total);
        }
    }
}
