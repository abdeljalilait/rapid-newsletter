using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;
using Resend;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class ResendEmailProvider : IEmailProvider
{
    private readonly IResend _client;

    public string ProviderName => "Resend";
    public EmailProvider Provider => EmailProvider.Resend;

    public ResendEmailProvider(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));

        _client = ResendClient.Create(apiKey);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var email = new EmailMessage
        {
            From = message.FromEmail,
            To = { message.ToEmail },
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody
        };

        var response = await _client.EmailSendAsync(email, ct);

        return response.Success
            ? new SendEmailResult(true, response.Content.ToString())
            : new SendEmailResult(false, Error: response.Exception?.Message);
    }
}
