using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;
using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class BrevoEmailProvider : IEmailProvider
{
    private readonly TransactionalEmailsApi _api;

    public string ProviderName => "Brevo";
    public EmailProvider Provider => EmailProvider.Brevo;

    public BrevoEmailProvider(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));

        var configuration = new brevo_csharp.Client.Configuration { BasePath = "https://api.brevo.com/v3" };
        configuration.AddApiKey("api-key", apiKey);
        _api = new TransactionalEmailsApi(configuration);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var email = new SendSmtpEmail
        {
            Sender = new SendSmtpEmailSender(message.FromName, message.FromEmail, null),
            To = new List<SendSmtpEmailTo> { new(message.ToEmail, null) },
            Subject = message.Subject,
            HtmlContent = message.HtmlBody,
            TextContent = message.TextBody
        };

        var response = await _api.SendTransacEmailAsync(email);
        return new SendEmailResult(true, response?.MessageId);
    }
}
