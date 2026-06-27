using NewsletterPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NewsletterPlatform.Infrastructure.Persistence;

public class NewsletterPlatformDbContext : DbContext
{
    public NewsletterPlatformDbContext(DbContextOptions<NewsletterPlatformDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Subscriber> Subscribers => Set<Subscriber>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<SubscriberTag> SubscriberTags => Set<SubscriberTag>();
    public DbSet<AudienceList> AudienceLists => Set<AudienceList>();
    public DbSet<AudienceListMember> AudienceListMembers => Set<AudienceListMember>();
    public DbSet<Suppression> Suppressions => Set<Suppression>();
    public DbSet<UnsubscribePreference> UnsubscribePreferences => Set<UnsubscribePreference>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<WorkspacePaymentConfiguration> WorkspacePaymentConfigurations => Set<WorkspacePaymentConfiguration>();
    public DbSet<ReaderSubscription> ReaderSubscriptions => Set<ReaderSubscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignRecipient> CampaignRecipients => Set<CampaignRecipient>();
    public DbSet<CampaignProviderAccount> CampaignProviderAccounts => Set<CampaignProviderAccount>();
    public DbSet<EmailProviderAccount> EmailProviderAccounts => Set<EmailProviderAccount>();
    public DbSet<ProviderUsage> ProviderUsage => Set<ProviderUsage>();
    public DbSet<ProviderQuotaReservation> ProviderQuotaReservations => Set<ProviderQuotaReservation>();
    public DbSet<EmailEvent> EmailEvents => Set<EmailEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NewsletterPlatformDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(Guid) && property.IsPrimaryKey())
                {
                    property.ValueGenerated = ValueGenerated.Never;
                }
            }
        }

        base.OnModelCreating(modelBuilder);
    }
}
