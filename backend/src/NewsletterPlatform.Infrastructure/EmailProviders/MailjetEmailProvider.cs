using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;
using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Mailjet.Client.TransactionalEmails.Response;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class MailjetEmailProvider : IEmailProvider
{
    private readonly MailjetClient _client;

    public string ProviderName => "Mailjet";
    public EmailProvider Provider => EmailProvider.Mailjet;

    public MailjetEmailProvider(string apiKey, string apiSecret)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(apiSecret))
            throw new ArgumentException("API secret is required.", nameof(apiSecret));

        _client = new MailjetClient(apiKey, apiSecret);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var email = new TransactionalEmail
        {
            From = new SendContact(message.FromEmail, message.FromName),
            To = new List<SendContact> { new(message.ToEmail) },
            Subject = message.Subject,
            HTMLPart = message.HtmlBody,
            TextPart = message.TextBody
        };

        var response = await _client.SendTransactionalEmailAsync(email, true, false);

        var firstMessage = response?.Messages?.FirstOrDefault();
        var firstTo = firstMessage?.To?.FirstOrDefault();

        return firstMessage?.Status == "success"
            ? new SendEmailResult(true, firstTo?.MessageID.ToString())
            : new SendEmailResult(false, Error: ExtractError(firstMessage));
    }

    private static string? ExtractError(MessageResult? message)
    {
        if (message is null) return "Unknown Mailjet error";
        var error = message.Errors?.FirstOrDefault();
        return error?.ToString() ?? $"Status: {message.Status}";
    }
}
