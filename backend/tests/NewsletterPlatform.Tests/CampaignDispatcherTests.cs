using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using NewsletterPlatform.Infrastructure.Persistence;
using NewsletterPlatform.Infrastructure.Persistence.Repositories;
using NewsletterPlatform.Infrastructure.Services;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class CampaignDispatcherTests
{
    [Fact]
    public async Task DispatchPendingCampaignsAsync_SendsAllRecipients_AndCompletesCampaign()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var campaign = SeedCampaign(db, workspaceId);
        var provider = SeedProvider(db, workspaceId, "Primary");
        SeedSubscribers(db, workspaceId, 3);
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        var launch = await repo.LaunchCampaignAsync(workspaceId, campaign.Id, new CampaignLaunchRequest(
            [new SelectedProviderAccountRequest(provider.Id, Priority: 1, RatePerMinute: 10, MaximumEmails: 100, Enabled: true)],
            AllowPartialCampaign: false));
        await db.SaveChangesAsync();

        Assert.Null(launch!.Error);

        var emailSender = new FakeEmailSender();
        var dispatcher = CreateDispatcher(db, repo, emailSender);

        await dispatcher.DispatchPendingCampaignsAsync();

        var recipients = await db.CampaignRecipients.Where(r => r.CampaignId == campaign.Id).ToArrayAsync();
        Assert.All(recipients, r => Assert.Equal(CampaignRecipientStatus.Sent, r.Status));
        Assert.Equal(3, emailSender.SentEmails.Count);

        var updatedCampaign = await db.Campaigns.SingleAsync(c => c.Id == campaign.Id);
        Assert.Equal(CampaignStatus.Completed, updatedCampaign.Status);
    }

    [Fact]
    public async Task DispatchPendingCampaignsAsync_MarksCampaignPartiallyCompleted_WhenSomeSendsFail()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var campaign = SeedCampaign(db, workspaceId);
        var provider = SeedProvider(db, workspaceId, "Primary");
        SeedSubscribers(db, workspaceId, 3);
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        var launch = await repo.LaunchCampaignAsync(workspaceId, campaign.Id, new CampaignLaunchRequest(
            [new SelectedProviderAccountRequest(provider.Id, Priority: 1, RatePerMinute: 10, MaximumEmails: 100, Enabled: true)],
            AllowPartialCampaign: false));
        await db.SaveChangesAsync();

        Assert.Null(launch!.Error);

        var emailSender = new FakeEmailSender();
        emailSender.SetFailureFor("reader2@example.com");

        var dispatcher = CreateDispatcher(db, repo, emailSender);
        await dispatcher.DispatchPendingCampaignsAsync();

        var recipients = await db.CampaignRecipients.Where(r => r.CampaignId == campaign.Id).ToArrayAsync();
        Assert.Equal(2, recipients.Count(r => r.Status == CampaignRecipientStatus.Sent));
        Assert.Equal(1, recipients.Count(r => r.Status == CampaignRecipientStatus.Failed));

        var updatedCampaign = await db.Campaigns.SingleAsync(c => c.Id == campaign.Id);
        Assert.Equal(CampaignStatus.PartiallyCompleted, updatedCampaign.Status);
    }

    [Fact]
    public async Task DispatchPendingCampaignsAsync_DoesNotProcessDraftCampaigns()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var campaign = SeedCampaign(db, workspaceId, CampaignStatus.Draft);
        SeedSubscribers(db, workspaceId, 2);
        await db.SaveChangesAsync();

        var emailSender = new FakeEmailSender();
        var dispatcher = CreateDispatcher(db, CreateRepository(db), emailSender);

        await dispatcher.DispatchPendingCampaignsAsync();

        Assert.Empty(emailSender.SentEmails);
    }

    [Fact]
    public async Task DispatchPendingCampaignsAsync_RespectsScheduledTime()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var campaign = SeedCampaign(db, workspaceId, CampaignStatus.Preparing, scheduledAt: DateTime.UtcNow.AddHours(1));
        var provider = SeedProvider(db, workspaceId, "Primary");
        SeedSubscribers(db, workspaceId, 2);
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        await repo.LaunchCampaignAsync(workspaceId, campaign.Id, new CampaignLaunchRequest(
            [new SelectedProviderAccountRequest(provider.Id, Priority: 1, RatePerMinute: 10, MaximumEmails: 100, Enabled: true)],
            AllowPartialCampaign: false));
        await db.SaveChangesAsync();

        var emailSender = new FakeEmailSender();
        var dispatcher = CreateDispatcher(db, repo, emailSender);

        await dispatcher.DispatchPendingCampaignsAsync();

        Assert.Empty(emailSender.SentEmails);
    }

    private static NewsletterPlatformDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<NewsletterPlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new NewsletterPlatformDbContext(options);
    }

    private static NewsletterRepository CreateRepository(NewsletterPlatformDbContext db) =>
        new(db, new TestSecretProtector());

    private static CampaignDispatcher CreateDispatcher(
        NewsletterPlatformDbContext db,
        NewsletterRepository repo,
        IEmailSender emailSender)
    {
        return new CampaignDispatcher(
            db,
            repo,
            new UnitOfWork(db),
            emailSender,
            new SystemDateTimeProvider(),
            NullLogger<CampaignDispatcher>.Instance,
            Options.Create(new CampaignDispatchWorkerOptions { BatchSize = 50 }));
    }

    private static Campaign SeedCampaign(
        NewsletterPlatformDbContext db,
        Guid workspaceId,
        CampaignStatus status = CampaignStatus.Draft,
        DateTime? scheduledAt = null)
    {
        var campaign = new Campaign
        {
            WorkspaceId = workspaceId,
            Name = "Weekly Campaign",
            Subject = "This week",
            FromName = "Editor",
            FromEmail = "editor@example.com",
            AudienceFilterJson = "{}",
            Status = status,
            ScheduledAt = scheduledAt
        };
        db.Campaigns.Add(campaign);
        return campaign;
    }

    private static EmailProviderAccount SeedProvider(NewsletterPlatformDbContext db, Guid workspaceId, string accountName)
    {
        var provider = new EmailProviderAccount
        {
            WorkspaceId = workspaceId,
            Provider = EmailProvider.Resend,
            AccountName = accountName,
            EncryptedApiKey = $"protected:{accountName}",
            FromName = accountName,
            FromEmail = $"{accountName.ToLowerInvariant()}@example.com",
            Enabled = true,
            HealthStatus = ProviderHealthStatus.Active,
            RatePerMinute = 10
        };
        db.EmailProviderAccounts.Add(provider);
        return provider;
    }

    private static void SeedSubscribers(NewsletterPlatformDbContext db, Guid workspaceId, int count)
    {
        for (var index = 1; index <= count; index++)
        {
            db.Subscribers.Add(new Subscriber
            {
                WorkspaceId = workspaceId,
                Email = $"reader{index}@example.com",
                Status = SubscriberStatus.Active,
                AccessLevel = SubscriberAccessLevel.Free,
                SubscribedAt = DateTime.UtcNow.AddMinutes(-index)
            });
        }
    }

    private sealed class FakeEmailSender : IEmailSender
    {
        private readonly HashSet<string> _failures = [];
        public List<(string To, string Subject)> SentEmails { get; } = [];

        public void SetFailureFor(string email) => _failures.Add(email);

        public Task SendAsync(string to, string subject, string htmlBody, string? plainBody = null, CancellationToken ct = default)
        {
            if (_failures.Contains(to))
                throw new InvalidOperationException($"Simulated failure for {to}");

            SentEmails.Add((to, subject));
            return Task.CompletedTask;
        }
    }

    private sealed class TestSecretProtector : ISecretProtector
    {
        public string Protect(string secret) => $"protected:{secret}";
        public string Unprotect(string protectedSecret) => protectedSecret["protected:".Length..];
    }
}
