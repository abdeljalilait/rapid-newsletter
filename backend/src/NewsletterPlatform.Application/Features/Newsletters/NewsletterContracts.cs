using System.Text.Json;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Features.Newsletters;

public sealed record PublicWorkspaceDto(Guid Id, string Name, string Slug, string? LogoUrl, string? Description);
public sealed record PublicSubscribeRequest(string Email, string? FirstName, string? LastName);
public sealed record PublicUnsubscribeRequest(string Email);
public sealed record PublicUnsubscribeDto(Guid SubscriberId, string Email, SubscriberStatus Status, bool Suppressed);
public sealed record NameRequest(string Name);
public sealed record ListRequest(string Name, string? Description);
public sealed record UpsertSubscriberRequest(string Email, string? FirstName, string? LastName, SubscriberStatus Status, SubscriberAccessLevel AccessLevel, string? ConsentSource, DateTime? ConsentAt);
public sealed record ImportSubscriberRow(string Email, string? FirstName, string? LastName, SubscriberAccessLevel AccessLevel);
public sealed record ImportSubscribersRequest(IReadOnlyList<ImportSubscriberRow> Rows);
public sealed record ImportSummaryDto(int RowsUploaded, int Imported, int Duplicates, int Invalid, int Skipped);
public sealed record PlanRequest(string Name, string? Description, decimal Price, string Currency, BillingInterval BillingInterval, string? DodoProductId, IReadOnlyList<string>? Benefits, bool IsActive, int SortOrder);
public sealed record PaymentConfigurationRequest(string ApiKey, string WebhookSecret, PaymentEnvironment Environment);
public sealed record ProviderAccountRequest(EmailProvider Provider, string AccountName, string ApiKey, string FromName, string FromEmail, string? SendingDomain, int? DailyLimit, int? MonthlyLimit, int RatePerMinute, bool Enabled);
public sealed record PaymentWebhookRequest(string ProviderEventId, string EventType, JsonElement RawPayload);
public sealed record PostRequest(string Title, string? Slug, string? Subtitle, string? PreviewText, string? CoverImageUrl, string EditorContentJson, string RenderedHtml, string PlainText, PostAudience Audience, PostStatus Status, bool PublishOnWebsite, bool SendByEmail, DateTime? ScheduledAt);
public sealed record AudienceFilterDto(SubscriberStatus? Status = null, SubscriberAccessLevel? AccessLevel = null, IReadOnlyList<Guid>? TagIds = null, IReadOnlyList<Guid>? ListIds = null, DateTime? JoinedFrom = null, DateTime? JoinedTo = null);
public sealed record CampaignRequest(Guid? PostId, string Name, string Subject, string? PreviewText, string FromName, string FromEmail, string? ReplyTo, string BodyHtml, string PlainText, AudienceFilterDto AudienceFilter, DateTime? ScheduledAt, bool AllowPartialCampaign);
public sealed record SelectedProviderAccountRequest(Guid ProviderAccountId, int Priority, int RatePerMinute, int? MaximumEmails, bool Enabled);
public sealed record CampaignLaunchRequest(IReadOnlyList<SelectedProviderAccountRequest> ProviderAccounts, bool AllowPartialCampaign);
public sealed record CampaignCapacityDto(int FinalRecipients, int TotalSelectedCapacity, int MissingCapacity, int CombinedRatePerMinute, IReadOnlyCollection<ProviderCapacityDto> Accounts);
public sealed record ProviderCapacityDto(Guid ProviderAccountId, string AccountName, int RemainingCapacity, int RatePerMinute);
public sealed record CampaignDispatchClaimRequest(int MaxItems = 10);
public sealed record CampaignDispatchBatchDto(Guid CampaignId, int ClaimedCount, int RemainingQueued, IReadOnlyCollection<CampaignDispatchRecipientDto> Recipients);
public sealed record CampaignDispatchRecipientDto(Guid CampaignRecipientId, Guid SubscriberId, string Email, Guid ProviderAccountId, string ProviderAccountName, EmailProvider Provider, int Priority);
public sealed record OverviewAnalyticsDto(int TotalSubscribers, int FreeSubscribers, int PaidSubscribers, int ActiveSubscriptions, int EmailsSentThisMonth);
public sealed record CreatedDto<T>(T Value, bool Created);
public sealed record LaunchCampaignResult(CampaignDto? Campaign, string? Error, int? Recipients = null, int? Capacity = null);

public sealed record SubscriberDto(Guid Id, string Email, string? FirstName, string? LastName, SubscriberStatus Status, SubscriberAccessLevel AccessLevel, DateTime SubscribedAt)
{
    public static SubscriberDto From(Subscriber x) => new(x.Id, x.Email, x.FirstName, x.LastName, x.Status, x.AccessLevel, x.SubscribedAt);
}

public sealed record TagDto(Guid Id, string Name, string Slug)
{
    public static TagDto From(Tag x) => new(x.Id, x.Name, x.Slug);
}

public sealed record ListDto(Guid Id, string Name, string? Description)
{
    public static ListDto From(AudienceList x) => new(x.Id, x.Name, x.Description);
}

public sealed record PlanDto(Guid Id, string Name, string? Description, decimal Price, string Currency, BillingInterval BillingInterval, string? DodoProductId, bool IsActive, int SortOrder)
{
    public static PlanDto From(SubscriptionPlan x) => new(x.Id, x.Name, x.Description, x.Price, x.Currency, x.BillingInterval, x.DodoProductId, x.IsActive, x.SortOrder);
}

public sealed record PaymentConfigurationDto(Guid Id, PaymentEnvironment Environment, ConnectionStatus ConnectionStatus, DateTime? LastValidatedAt, bool HasApiKey, bool HasWebhookSecret)
{
    public static PaymentConfigurationDto From(WorkspacePaymentConfiguration x) => new(x.Id, x.Environment, x.ConnectionStatus, x.LastValidatedAt, true, true);
}

public sealed record ProviderAccountDto(Guid Id, EmailProvider Provider, string AccountName, string FromName, string FromEmail, string? SendingDomain, int? DailyLimit, int? MonthlyLimit, int RatePerMinute, bool Enabled, ProviderHealthStatus HealthStatus, DateTime? LastValidatedAt, DateTime? LastSuccessfulSendAt, string? LastError, bool HasApiKey)
{
    public static ProviderAccountDto From(EmailProviderAccount x) => new(x.Id, x.Provider, x.AccountName, x.FromName, x.FromEmail, x.SendingDomain, x.DailyLimit, x.MonthlyLimit, x.RatePerMinute, x.Enabled, x.HealthStatus, x.LastValidatedAt, x.LastSuccessfulSendAt, x.LastError, true);
}

public sealed record PostDto(Guid Id, string Title, string Slug, string? Subtitle, string? PreviewText, string? CoverImageUrl, PostAudience Audience, PostStatus Status, bool PublishOnWebsite, bool SendByEmail, DateTime? ScheduledAt, DateTime? PublishedAt)
{
    public static PostDto From(Post x) => new(x.Id, x.Title, x.Slug, x.Subtitle, x.PreviewText, x.CoverImageUrl, x.Audience, x.Status, x.PublishOnWebsite, x.SendByEmail, x.ScheduledAt, x.PublishedAt);
}

public sealed record PublicPostDetailDto(Guid Id, string Title, string Slug, string? Subtitle, string? PreviewText, string? CoverImageUrl, PostAudience Audience, PostStatus Status, DateTime? PublishedAt, string? RenderedHtml, string? PlainText)
{
    public static PublicPostDetailDto From(Post x) => new(x.Id, x.Title, x.Slug, x.Subtitle, x.PreviewText, x.CoverImageUrl, x.Audience, x.Status, x.PublishedAt, x.RenderedHtml, x.PlainText);
}

public sealed record CampaignDto(Guid Id, Guid? PostId, string Name, string Subject, CampaignStatus Status, DateTime? ScheduledAt, bool AllowPartialCampaign, int RecipientCount)
{
    public static CampaignDto From(Campaign x) => new(x.Id, x.PostId, x.Name, x.Subject, x.Status, x.ScheduledAt, x.AllowPartialCampaign, x.RecipientCount);
}
