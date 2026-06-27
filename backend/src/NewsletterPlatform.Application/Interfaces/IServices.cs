using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Application.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(
        Guid? workspaceId,
        Guid? actorUserId,
        string action,
        string? entityType = null,
        Guid? entityId = null,
        object? metadata = null,
        string? ipAddress = null,
        CancellationToken ct = default);
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string? plainBody = null, CancellationToken ct = default);
}

public interface IEmailProvider
{
    string ProviderName { get; }
    EmailProvider Provider { get; }
    Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default);
}

public sealed record ProviderEmailMessage(
    string FromEmail,
    string FromName,
    string ToEmail,
    string Subject,
    string HtmlBody,
    string? TextBody = null);

public sealed record SendEmailResult(bool Success, string? ProviderMessageId = null, string? Error = null);

public interface IEmailProviderFactory
{
    IEmailProvider Create(EmailProviderAccount account);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    IReadOnlyCollection<string> Roles { get; }
}

public interface IExecutionContext
{
    ICurrentUserService Current { get; }
    IDateTimeProvider Time { get; }
}

public interface ICampaignDispatcher
{
    Task DispatchPendingCampaignsAsync(CancellationToken ct = default);
}
