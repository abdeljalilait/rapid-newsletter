using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NewsletterPlatform.Domain.Entities;

namespace NewsletterPlatform.Infrastructure.Persistence.Configuration;

public class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
{
    public void Configure(EntityTypeBuilder<Subscriber> builder)
    {
        builder.ToTable("subscribers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100);
        builder.Property(x => x.LastName).HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.AccessLevel).HasConversion<int>().IsRequired();
        builder.Property(x => x.ConsentSource).HasMaxLength(200);
        builder.HasIndex(x => new { x.WorkspaceId, x.Email }).IsUnique();
        builder.HasIndex(x => new { x.WorkspaceId, x.Status, x.AccessLevel });
    }
}

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(140).IsRequired();
        builder.HasIndex(x => new { x.WorkspaceId, x.Slug }).IsUnique();
    }
}

public class SubscriberTagConfiguration : IEntityTypeConfiguration<SubscriberTag>
{
    public void Configure(EntityTypeBuilder<SubscriberTag> builder)
    {
        builder.ToTable("subscriber_tags");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkspaceId, x.SubscriberId, x.TagId }).IsUnique();
    }
}

public class AudienceListConfiguration : IEntityTypeConfiguration<AudienceList>
{
    public void Configure(EntityTypeBuilder<AudienceList> builder)
    {
        builder.ToTable("audience_lists");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.WorkspaceId, x.Name }).IsUnique();
    }
}

public class AudienceListMemberConfiguration : IEntityTypeConfiguration<AudienceListMember>
{
    public void Configure(EntityTypeBuilder<AudienceListMember> builder)
    {
        builder.ToTable("audience_list_members");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkspaceId, x.AudienceListId, x.SubscriberId }).IsUnique();
    }
}

public class SuppressionConfiguration : IEntityTypeConfiguration<Suppression>
{
    public void Configure(EntityTypeBuilder<Suppression> builder)
    {
        builder.ToTable("suppressions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Reason).HasConversion<int>().IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.WorkspaceId, x.Email }).IsUnique();
    }
}

public class UnsubscribePreferenceConfiguration : IEntityTypeConfiguration<UnsubscribePreference>
{
    public void Configure(EntityTypeBuilder<UnsubscribePreference> builder)
    {
        builder.ToTable("unsubscribe_preferences");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkspaceId, x.SubscriberId, x.AudienceListId }).IsUnique();
    }
}

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Price).HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.BillingInterval).HasConversion<int>().IsRequired();
        builder.Property(x => x.DodoProductId).HasMaxLength(200);
        builder.Property(x => x.BenefitsJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.WorkspaceId, x.SortOrder });
    }
}

public class WorkspacePaymentConfigurationConfiguration : IEntityTypeConfiguration<WorkspacePaymentConfiguration>
{
    public void Configure(EntityTypeBuilder<WorkspacePaymentConfiguration> builder)
    {
        builder.ToTable("workspace_payment_configurations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EncryptedApiKey).IsRequired();
        builder.Property(x => x.EncryptedWebhookSecret).IsRequired();
        builder.Property(x => x.Environment).HasConversion<int>().IsRequired();
        builder.Property(x => x.ConnectionStatus).HasConversion<int>().IsRequired();
        builder.HasIndex(x => x.WorkspaceId).IsUnique();
    }
}

public class ReaderSubscriptionConfiguration : IEntityTypeConfiguration<ReaderSubscription>
{
    public void Configure(EntityTypeBuilder<ReaderSubscription> builder)
    {
        builder.ToTable("reader_subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DodoCustomerId).HasMaxLength(200);
        builder.Property(x => x.DodoSubscriptionId).HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.WorkspaceId, x.SubscriberId });
        builder.HasIndex(x => new { x.WorkspaceId, x.DodoSubscriptionId });
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DodoPaymentId).HasMaxLength(200);
        builder.Property(x => x.Amount).HasPrecision(12, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.WorkspaceId, x.SubscriberId });
        builder.HasIndex(x => new { x.WorkspaceId, x.DodoPaymentId });
    }
}

public class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.ToTable("payment_webhook_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderEventId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(160).IsRequired();
        builder.Property(x => x.RawPayload).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.ProcessingStatus).HasConversion<int>().IsRequired();
        builder.Property(x => x.Error).HasMaxLength(4000);
        builder.HasIndex(x => new { x.WorkspaceId, x.ProviderEventId }).IsUnique();
    }
}

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(240).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Subtitle).HasMaxLength(500);
        builder.Property(x => x.PreviewText).HasMaxLength(500);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(1024);
        builder.Property(x => x.EditorContentJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Audience).HasConversion<int>().IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.WorkspaceId, x.Slug }).IsUnique();
        builder.HasIndex(x => new { x.WorkspaceId, x.Status, x.PublishedAt });
    }
}

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(240).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(240).IsRequired();
        builder.Property(x => x.PreviewText).HasMaxLength(500);
        builder.Property(x => x.FromName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.FromEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ReplyTo).HasMaxLength(256);
        builder.Property(x => x.AudienceFilterJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(x => new { x.WorkspaceId, x.Status, x.ScheduledAt });
    }
}

public class CampaignRecipientConfiguration : IEntityTypeConfiguration<CampaignRecipient>
{
    public void Configure(EntityTypeBuilder<CampaignRecipient> builder)
    {
        builder.ToTable("campaign_recipients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.HasIndex(x => new { x.WorkspaceId, x.CampaignId, x.SubscriberId }).IsUnique();
        builder.HasIndex(x => new { x.WorkspaceId, x.CampaignId, x.Status });
    }
}

public class CampaignProviderAccountConfiguration : IEntityTypeConfiguration<CampaignProviderAccount>
{
    public void Configure(EntityTypeBuilder<CampaignProviderAccount> builder)
    {
        builder.ToTable("campaign_provider_accounts");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkspaceId, x.CampaignId, x.ProviderAccountId }).IsUnique();
    }
}

public class EmailProviderAccountConfiguration : IEntityTypeConfiguration<EmailProviderAccount>
{
    public void Configure(EntityTypeBuilder<EmailProviderAccount> builder)
    {
        builder.ToTable("email_provider_accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasConversion<int>().IsRequired();
        builder.Property(x => x.AccountName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.EncryptedApiKey).IsRequired();
        builder.Property(x => x.FromName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.FromEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.SendingDomain).HasMaxLength(256);
        builder.Property(x => x.HealthStatus).HasConversion<int>().IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.HasIndex(x => new { x.WorkspaceId, x.AccountName }).IsUnique();
    }
}

public class ProviderUsageConfiguration : IEntityTypeConfiguration<ProviderUsage>
{
    public void Configure(EntityTypeBuilder<ProviderUsage> builder)
    {
        builder.ToTable("provider_usage");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkspaceId, x.ProviderAccountId, x.UsageDate }).IsUnique();
    }
}

public class ProviderQuotaReservationConfiguration : IEntityTypeConfiguration<ProviderQuotaReservation>
{
    public void Configure(EntityTypeBuilder<ProviderQuotaReservation> builder)
    {
        builder.ToTable("provider_quota_reservations");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.WorkspaceId, x.ProviderAccountId, x.CampaignId });
    }
}

public class EmailEventConfiguration : IEntityTypeConfiguration<EmailEvent>
{
    public void Configure(EntityTypeBuilder<EmailEvent> builder)
    {
        builder.ToTable("email_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderMessageId).HasMaxLength(200);
        builder.Property(x => x.EventType).HasConversion<int>().IsRequired();
        builder.Property(x => x.LinkUrl).HasMaxLength(2048);
        builder.Property(x => x.MetadataJson).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.WorkspaceId, x.CampaignId, x.EventType });
        builder.HasIndex(x => new { x.WorkspaceId, x.SubscriberId });
    }
}
