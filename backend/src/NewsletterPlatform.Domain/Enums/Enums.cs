namespace NewsletterPlatform.Domain.Enums;

public enum WorkspaceRole
{
    Owner = 0,
    Admin = 1,
    Editor = 2,
    Viewer = 3
}

public enum WorkspaceStatus
{
    Active = 0,
    Suspended = 1,
    Deleted = 2
}

public enum RefreshTokenStatus
{
    Active = 0,
    Revoked = 1,
    Used = 2,
    Expired = 3
}

public enum SubscriberStatus
{
    PendingConfirmation = 0,
    Active = 1,
    Unsubscribed = 2,
    HardBounced = 3,
    SoftBounced = 4,
    Complained = 5,
    Suppressed = 6,
    Invalid = 7
}

public enum SubscriberAccessLevel
{
    Free = 0,
    Paid = 1,
    Complimentary = 2,
    Lifetime = 3
}

public enum BillingInterval
{
    Free = 0,
    Monthly = 1,
    Annual = 2,
    OneTime = 3
}

public enum PaymentEnvironment
{
    Test = 0,
    Live = 1
}

public enum ConnectionStatus
{
    PendingValidation = 0,
    Active = 1,
    InvalidCredentials = 2,
    Disabled = 3
}

public enum ReaderSubscriptionStatus
{
    Pending = 0,
    Active = 1,
    PastDue = 2,
    OnHold = 3,
    Cancelled = 4,
    Expired = 5
}

public enum PostAudience
{
    Public = 0,
    FreeSubscribers = 1,
    PaidSubscribers = 2,
    SpecificList = 3,
    SpecificTags = 4,
    CustomFilter = 5
}

public enum PostStatus
{
    Draft = 0,
    Scheduled = 1,
    Published = 2,
    Archived = 3
}

public enum CampaignStatus
{
    Draft = 0,
    Preparing = 1,
    Scheduled = 2,
    Sending = 3,
    Paused = 4,
    Completed = 5,
    PartiallyCompleted = 6,
    Cancelled = 7,
    Failed = 8
}

public enum CampaignRecipientStatus
{
    Queued = 0,
    Sending = 1,
    Sent = 2,
    Delivered = 3,
    Opened = 4,
    Clicked = 5,
    SoftBounced = 6,
    HardBounced = 7,
    Complained = 8,
    Unsubscribed = 9,
    Failed = 10,
    Skipped = 11
}

public enum EmailProvider
{
    Resend = 0,
    Mailtrap = 1,
    Sender = 2,
    Brevo = 3,
    Mailjet = 4,
    Mailgun = 5,
    Loops = 6,
    Smtp2Go = 7
}

public enum ProviderHealthStatus
{
    PendingValidation = 0,
    Active = 1,
    InvalidCredentials = 2,
    QuotaExhausted = 3,
    TemporarilyUnavailable = 4,
    Disabled = 5
}

public enum EmailEventType
{
    Queued = 0,
    Sending = 1,
    Sent = 2,
    Delivered = 3,
    Opened = 4,
    Clicked = 5,
    SoftBounced = 6,
    HardBounced = 7,
    Complained = 8,
    Unsubscribed = 9,
    Failed = 10
}

public enum WebhookProcessingStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2,
    Ignored = 3
}

public enum SuppressionReason
{
    HardBounce = 0,
    SpamComplaint = 1,
    Manual = 2,
    GlobalUnsubscribe = 3
}
