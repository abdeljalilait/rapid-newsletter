using NewsletterPlatform.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace NewsletterPlatform.Infrastructure.Services;

public sealed class DevEmailSender : IEmailSender
{
    private readonly ILogger<DevEmailSender> _logger;
    public DevEmailSender(ILogger<DevEmailSender> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string htmlBody, string? plainBody = null, CancellationToken ct = default)
    {
        _logger.LogInformation("DEV EMAIL -> {To} | {Subject}\n{Plain}", to, subject, plainBody ?? htmlBody);
        return Task.CompletedTask;
    }
}
