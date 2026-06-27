using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;
using Smtp2Go.Api;
using Smtp2Go.Api.Models.Emails;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class Smtp2GoEmailProvider : IEmailProvider
{
    private readonly Smtp2GoApiService _service;

    public string ProviderName => "SMTP2GO";
    public EmailProvider Provider => EmailProvider.Smtp2Go;

    public Smtp2GoEmailProvider(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));

        _service = new Smtp2GoApiService(apiKey, "https://api.smtp2go.com/v3/");
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var email = new EmailMessage(
            $"{message.FromName} <{message.FromEmail}>",
            message.Subject,
            message.HtmlBody,
            new[] { message.ToEmail })
        {
            BodyText = message.TextBody
        };

        var response = await _service.SendEmail(email);

        return response.Data?.Succeeded > 0
            ? new SendEmailResult(true, response.Data.EmailId)
            : new SendEmailResult(false, Error: response.Data?.Error ?? response.ResponseStatus);
    }
}
