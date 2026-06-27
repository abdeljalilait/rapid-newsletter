using Microsoft.EntityFrameworkCore;
using NewsletterPlatform.Application.Features.Newsletters;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using NewsletterPlatform.Infrastructure.Persistence;
using NewsletterPlatform.Infrastructure.Persistence.Repositories;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class NewsletterRepositoryTests
{
    [Fact]
    public async Task HasWorkspaceRoleAsync_EnforcesRoleHierarchy()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var editorId = Guid.NewGuid();

        db.WorkspaceMembers.AddRange(
            new WorkspaceMember { WorkspaceId = workspaceId, UserId = ownerId, Role = WorkspaceRole.Owner },
            new WorkspaceMember { WorkspaceId = workspaceId, UserId = editorId, Role = WorkspaceRole.Editor });
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);

        Assert.True(await repo.HasWorkspaceRoleAsync(ownerId, workspaceId, WorkspaceRole.Owner));
        Assert.True(await repo.HasWorkspaceRoleAsync(ownerId, workspaceId, WorkspaceRole.Admin));
        Assert.True(await repo.HasWorkspaceRoleAsync(ownerId, workspaceId, WorkspaceRole.Editor));
        Assert.True(await repo.HasWorkspaceRoleAsync(ownerId, workspaceId, WorkspaceRole.Viewer));
        Assert.True(await repo.HasWorkspaceRoleAsync(editorId, workspaceId, WorkspaceRole.Editor));
        Assert.False(await repo.HasWorkspaceRoleAsync(editorId, workspaceId, WorkspaceRole.Admin));
        Assert.False(await repo.HasWorkspaceRoleAsync(Guid.NewGuid(), workspaceId, WorkspaceRole.Viewer));
    }

    [Fact]
    public async Task LaunchCampaignAsync_BlocksInsufficientCapacity_WhenPartialIsNotAllowed()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var campaign = SeedCampaign(db, workspaceId);
        var provider = SeedProvider(db, workspaceId, "Primary");
        SeedSubscribers(db, workspaceId, 3);
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        var result = await repo.LaunchCampaignAsync(workspaceId, campaign.Id, new CampaignLaunchRequest(
            [new SelectedProviderAccountRequest(provider.Id, Priority: 1, RatePerMinute: 10, MaximumEmails: 2, Enabled: true)],
            AllowPartialCampaign: false));
        await db.SaveChangesAsync();

        Assert.NotNull(result);
        Assert.Equal("Selected provider capacity is insufficient.", result!.Error);
        Assert.Equal(3, result.Recipients);
        Assert.Equal(2, result.Capacity);
        Assert.Equal(CampaignStatus.Draft, campaign.Status);
        Assert.Empty(await db.CampaignRecipients.Where(x => x.CampaignId == campaign.Id).ToArrayAsync());
        Assert.Empty(await db.CampaignProviderAccounts.Where(x => x.CampaignId == campaign.Id).ToArrayAsync());
    }

    [Fact]
    public async Task ClaimCampaignDispatchBatchAsync_RotatesAcrossSelectedProviders()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var campaign = SeedCampaign(db, workspaceId);
        var firstProvider = SeedProvider(db, workspaceId, "First");
        var secondProvider = SeedProvider(db, workspaceId, "Second");
        SeedSubscribers(db, workspaceId, 4);
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        var launch = await repo.LaunchCampaignAsync(workspaceId, campaign.Id, new CampaignLaunchRequest(
            [
                new SelectedProviderAccountRequest(firstProvider.Id, Priority: 1, RatePerMinute: 2, MaximumEmails: 10, Enabled: true),
                new SelectedProviderAccountRequest(secondProvider.Id, Priority: 1, RatePerMinute: 2, MaximumEmails: 10, Enabled: true)
            ],
            AllowPartialCampaign: false));
        await db.SaveChangesAsync();

        Assert.Null(launch!.Error);

        var firstBatch = await repo.ClaimCampaignDispatchBatchAsync(workspaceId, campaign.Id, new CampaignDispatchClaimRequest(MaxItems: 2));
        await db.SaveChangesAsync();
        var secondBatch = await repo.ClaimCampaignDispatchBatchAsync(workspaceId, campaign.Id, new CampaignDispatchClaimRequest(MaxItems: 2));
        await db.SaveChangesAsync();

        Assert.Equal(2, firstBatch!.ClaimedCount);
        Assert.Equal(2, secondBatch!.ClaimedCount);
        Assert.Equal(0, secondBatch.RemainingQueued);

        var allClaimed = firstBatch.Recipients.Concat(secondBatch.Recipients).ToArray();
        Assert.Equal(2, allClaimed.Count(x => x.ProviderAccountId == firstProvider.Id));
        Assert.Equal(2, allClaimed.Count(x => x.ProviderAccountId == secondProvider.Id));

        var recipients = await db.CampaignRecipients.Where(x => x.CampaignId == campaign.Id).ToArrayAsync();
        Assert.All(recipients, recipient => Assert.Equal(CampaignRecipientStatus.Sending, recipient.Status));
        Assert.All(recipients, recipient => Assert.NotNull(recipient.ProviderAccountId));
        Assert.Equal(2, await db.CampaignProviderAccounts.SingleAsync(x => x.ProviderAccountId == firstProvider.Id).ContinueWith(x => x.Result.SentCount));
        Assert.Equal(2, await db.CampaignProviderAccounts.SingleAsync(x => x.ProviderAccountId == secondProvider.Id).ContinueWith(x => x.Result.SentCount));
    }

    [Fact]
    public async Task UpsertPaymentConfigurationAsync_StoresProtectedSecrets_AndReturnsOnlySecretPresence()
    {
        await using var db = CreateDb();
        var workspaceId = Guid.NewGuid();
        var repo = CreateRepository(db);

        var dto = await repo.UpsertPaymentConfigurationAsync(workspaceId, new PaymentConfigurationRequest(
            ApiKey: "dodo-api-key",
            WebhookSecret: "webhook-secret",
            Environment: PaymentEnvironment.Test));
        await db.SaveChangesAsync();

        var saved = await db.WorkspacePaymentConfigurations.SingleAsync(x => x.WorkspaceId == workspaceId);
        Assert.Equal(PaymentEnvironment.Test, dto.Environment);
        Assert.True(dto.HasApiKey);
        Assert.True(dto.HasWebhookSecret);
        Assert.Equal("protected:dodo-api-key", saved.EncryptedApiKey);
        Assert.Equal("protected:webhook-secret", saved.EncryptedWebhookSecret);
    }

    [Fact]
    public async Task UnsubscribePublicAsync_MarksSubscriberAndSuppressesEmail()
    {
        await using var db = CreateDb();
        var workspace = SeedWorkspace(db, "dotnet-weekly");
        var subscriber = new Subscriber
        {
            WorkspaceId = workspace.Id,
            Email = "reader@example.com",
            Status = SubscriberStatus.Active,
            AccessLevel = SubscriberAccessLevel.Free
        };
        db.Subscribers.Add(subscriber);
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        var result = await repo.UnsubscribePublicAsync(workspace.Slug, new PublicUnsubscribeRequest("READER@example.com"));
        await db.SaveChangesAsync();

        Assert.NotNull(result);
        Assert.Equal(SubscriberStatus.Unsubscribed, result!.Status);
        Assert.Equal(SubscriberStatus.Unsubscribed, (await db.Subscribers.SingleAsync()).Status);
        Assert.True(await db.UnsubscribePreferences.AnyAsync(x => x.SubscriberId == subscriber.Id && x.UnsubscribedFromAll));
        Assert.True(await db.Suppressions.AnyAsync(x =>
            x.WorkspaceId == workspace.Id &&
            x.Email == "reader@example.com" &&
            x.Reason == SuppressionReason.GlobalUnsubscribe));
    }

    [Fact]
    public async Task SubscribePublicAsync_ReactivatesUnsubscribedReaderAndClearsGlobalSuppression()
    {
        await using var db = CreateDb();
        var workspace = SeedWorkspace(db, "dotnet-weekly");
        var subscriber = new Subscriber
        {
            WorkspaceId = workspace.Id,
            Email = "reader@example.com",
            Status = SubscriberStatus.Unsubscribed,
            AccessLevel = SubscriberAccessLevel.Free
        };
        db.Subscribers.Add(subscriber);
        db.Suppressions.Add(new Suppression
        {
            WorkspaceId = workspace.Id,
            SubscriberId = subscriber.Id,
            Email = subscriber.Email,
            Reason = SuppressionReason.GlobalUnsubscribe
        });
        db.UnsubscribePreferences.Add(new UnsubscribePreference
        {
            WorkspaceId = workspace.Id,
            SubscriberId = subscriber.Id,
            UnsubscribedFromAll = true
        });
        await db.SaveChangesAsync();

        var repo = CreateRepository(db);
        var result = await repo.SubscribePublicAsync(workspace.Slug, new PublicSubscribeRequest(subscriber.Email, "Reader", null));
        await db.SaveChangesAsync();

        Assert.NotNull(result);
        Assert.False(result!.Created);
        Assert.Equal(SubscriberStatus.Active, (await db.Subscribers.SingleAsync()).Status);
        Assert.Empty(await db.Suppressions.ToArrayAsync());
        Assert.Empty(await db.UnsubscribePreferences.ToArrayAsync());
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

    private static Campaign SeedCampaign(NewsletterPlatformDbContext db, Guid workspaceId)
    {
        var campaign = new Campaign
        {
            WorkspaceId = workspaceId,
            Name = "Weekly Campaign",
            Subject = "This week",
            FromName = "Editor",
            FromEmail = "editor@example.com",
            AudienceFilterJson = "{}",
            Status = CampaignStatus.Draft
        };
        db.Campaigns.Add(campaign);
        return campaign;
    }

    private static Workspace SeedWorkspace(NewsletterPlatformDbContext db, string slug)
    {
        var workspace = new Workspace
        {
            Name = "Dotnet Weekly",
            Slug = slug,
            DefaultSenderName = "Editor",
            DefaultSenderEmail = "editor@example.com",
            Timezone = "UTC",
            DefaultCurrency = "USD"
        };
        db.Workspaces.Add(workspace);
        return workspace;
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
            RatePerMinute = 2
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

    private sealed class TestSecretProtector : ISecretProtector
    {
        public string Protect(string secret) => $"protected:{secret}";
        public string Unprotect(string protectedSecret) => protectedSecret["protected:".Length..];
    }
}
