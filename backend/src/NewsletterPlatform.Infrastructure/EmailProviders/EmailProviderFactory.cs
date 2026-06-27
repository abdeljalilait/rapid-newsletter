using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using NewsletterPlatform.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class EmailProviderFactory : IEmailProviderFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISecretProtector _protector;
    private readonly ILoggerFactory _loggerFactory;

    public EmailProviderFactory(IHttpClientFactory httpClientFactory, ISecretProtector protector, ILoggerFactory loggerFactory)
    {
        _httpClientFactory = httpClientFactory;
        _protector = protector;
        _loggerFactory = loggerFactory;
    }

    public IEmailProvider Create(EmailProviderAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        var apiKey = _protector.Unprotect(account.EncryptedApiKey);
        var apiSecret = string.IsNullOrEmpty(account.EncryptedApiSecret)
            ? null
            : _protector.Unprotect(account.EncryptedApiSecret);

        return account.Provider switch
        {
            EmailProvider.Resend => new ResendEmailProvider(apiKey),
            EmailProvider.Sender => new SenderEmailProvider(_httpClientFactory.CreateClient(), apiKey),
            EmailProvider.Brevo => new BrevoEmailProvider(apiKey),
            EmailProvider.Mailjet => new MailjetEmailProvider(apiKey, apiSecret ?? throw new InvalidOperationException("Mailjet requires an API secret.")),
            EmailProvider.Mailgun => new MailgunEmailProvider(_httpClientFactory.CreateClient(), apiKey, account.SendingDomain ?? throw new InvalidOperationException("Mailgun requires a sending domain.")),
            EmailProvider.Loops => new LoopsEmailProvider(_httpClientFactory.CreateClient(), apiKey, account.SendingDomain ?? throw new InvalidOperationException("Loops requires a transactional email ID in SendingDomain.")),
            EmailProvider.Smtp2Go => new Smtp2GoEmailProvider(apiKey),
            EmailProvider.Mailtrap => new DevEmailProvider(_loggerFactory.CreateLogger<DevEmailProvider>()),
            _ => throw new NotSupportedException($"Email provider '{account.Provider}' is not supported.")
        };
    }
}
