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
            EmailProvider.MailerSend => new MailerSendEmailProvider(_httpClientFactory.CreateClient(), apiKey),
            EmailProvider.Plunk => new PlunkEmailProvider(_httpClientFactory.CreateClient(), apiKey),
            EmailProvider.AmazonSes => new AmazonSesEmailProvider(
                apiKey,
                apiSecret ?? throw new InvalidOperationException("Amazon SES requires an API secret."),
                account.SendingDomain ?? throw new InvalidOperationException("Amazon SES requires an AWS region in SendingDomain.")),
            EmailProvider.Mailtrap => new DevEmailProvider(_loggerFactory.CreateLogger<DevEmailProvider>()),
            _ => throw new NotSupportedException($"Email provider '{account.Provider}' is not supported.")
        };
    }
}
