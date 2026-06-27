using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace NewsletterPlatform.Infrastructure.Services;

public sealed class DevEmailProvider : IEmailProvider
{
    private readonly ILogger<DevEmailProvider> _logger;

    public string ProviderName => "Dev";
    public EmailProvider Provider => EmailProvider.Resend;

    public DevEmailProvider(ILogger<DevEmailProvider> logger) => _logger = logger;

    public Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "DEV EMAIL -> {To} | {Subject}\n{Body}",
            message.ToEmail,
            message.Subject,
            message.TextBody ?? message.HtmlBody);

        return Task.FromResult(new SendEmailResult(true, ProviderMessageId: Guid.NewGuid().ToString("N")));
    }
}
