namespace NewsletterPlatform.Domain.Entities;

public class Subscriber : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public SubscriberStatus Status { get; set; } = SubscriberStatus.PendingConfirmation;
    public SubscriberAccessLevel AccessLevel { get; set; } = SubscriberAccessLevel.Free;
    public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastEngagementAt { get; set; }
    public string? ConsentSource { get; set; }
    public DateTime? ConsentAt { get; set; }
}

public class Tag : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
}

public class SubscriberTag : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid SubscriberId { get; set; }
    public Guid TagId { get; set; }
}

public class AudienceList : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class AudienceListMember : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid AudienceListId { get; set; }
    public Guid SubscriberId { get; set; }
}

public class Suppression : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid? SubscriberId { get; set; }
    public string Email { get; set; } = null!;
    public SuppressionReason Reason { get; set; }
    public string? Notes { get; set; }
}

public class UnsubscribePreference : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid SubscriberId { get; set; }
    public Guid? AudienceListId { get; set; }
    public bool UnsubscribedFromAll { get; set; }
    public DateTime UnsubscribedAt { get; set; } = DateTime.UtcNow;
}

public class SubscriptionPlan : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public BillingInterval BillingInterval { get; set; }
    public string? DodoProductId { get; set; }
    public string? BenefitsJson { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

public class WorkspacePaymentConfiguration : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string EncryptedApiKey { get; set; } = null!;
    public string EncryptedWebhookSecret { get; set; } = null!;
    public PaymentEnvironment Environment { get; set; } = PaymentEnvironment.Test;
    public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.PendingValidation;
    public DateTime? LastValidatedAt { get; set; }
}

public class ReaderSubscription : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid SubscriberId { get; set; }
    public Guid PlanId { get; set; }
    public string? DodoCustomerId { get; set; }
    public string? DodoSubscriptionId { get; set; }
    public ReaderSubscriptionStatus Status { get; set; } = ReaderSubscriptionStatus.Pending;
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
}

public class Payment : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid SubscriberId { get; set; }
    public Guid? ReaderSubscriptionId { get; set; }
    public Guid? PlanId { get; set; }
    public string? DodoPaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Status { get; set; } = "pending";
    public DateTime? PaidAt { get; set; }
}

public class PaymentWebhookEvent : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string ProviderEventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public string RawPayload { get; set; } = null!;
    public WebhookProcessingStatus ProcessingStatus { get; set; } = WebhookProcessingStatus.Pending;
    public string? Error { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class Post : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Subtitle { get; set; }
    public string? PreviewText { get; set; }
    public string? CoverImageUrl { get; set; }
    public string EditorContentJson { get; set; } = "{}";
    public string RenderedHtml { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public PostAudience Audience { get; set; } = PostAudience.Public;
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public bool PublishOnWebsite { get; set; } = true;
    public bool SendByEmail { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class Campaign : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid? PostId { get; set; }
    public string Name { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string? PreviewText { get; set; }
    public string FromName { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string? ReplyTo { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public string PlainText { get; set; } = string.Empty;
    public string AudienceFilterJson { get; set; } = "{}";
    public DateTime? ScheduledAt { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public bool AllowPartialCampaign { get; set; }
    public int RecipientCount { get; set; }
}

public class CampaignRecipient : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid SubscriberId { get; set; }
    public string Email { get; set; } = null!;
    public CampaignRecipientStatus Status { get; set; } = CampaignRecipientStatus.Queued;
    public int AttemptCount { get; set; }
    public Guid? ProviderAccountId { get; set; }
    public string? ProviderMessageId { get; set; }
    public string? LastError { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}

public class CampaignProviderAccount : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid ProviderAccountId { get; set; }
    public int Priority { get; set; } = 1;
    public int RatePerMinute { get; set; } = 1;
    public int? MaximumEmails { get; set; }
    public bool Enabled { get; set; } = true;
    public int SentCount { get; set; }
}

public class EmailProviderAccount : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public EmailProvider Provider { get; set; }
    public string AccountName { get; set; } = null!;
    public string EncryptedApiKey { get; set; } = null!;
    public string FromName { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string? SendingDomain { get; set; }
    public int? DailyLimit { get; set; }
    public int? MonthlyLimit { get; set; }
    public int RatePerMinute { get; set; } = 1;
    public bool Enabled { get; set; } = true;
    public ProviderHealthStatus HealthStatus { get; set; } = ProviderHealthStatus.PendingValidation;
    public DateTime? LastValidatedAt { get; set; }
    public DateTime? LastSuccessfulSendAt { get; set; }
    public string? LastError { get; set; }
    public bool IsDeleted { get; set; }
}

public class ProviderUsage : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid ProviderAccountId { get; set; }
    public DateOnly UsageDate { get; set; }
    public int DailySent { get; set; }
    public int MonthlySent { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
}

public class ProviderQuotaReservation : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid ProviderAccountId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid? CampaignRecipientId { get; set; }
    public bool Confirmed { get; set; }
    public bool Released { get; set; }
}

public class EmailEvent : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid CampaignId { get; set; }
    public Guid? CampaignRecipientId { get; set; }
    public Guid? SubscriberId { get; set; }
    public Guid? ProviderAccountId { get; set; }
    public string? ProviderMessageId { get; set; }
    public EmailEventType EventType { get; set; }
    public string? LinkUrl { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? MetadataJson { get; set; }
}
