using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class MailerSendEmailProvider : IEmailProvider
{
    private readonly HttpClient _client;

    public string ProviderName => "MailerSend";
    public EmailProvider Provider => EmailProvider.MailerSend;

    public MailerSendEmailProvider(HttpClient client, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));

        _client = client;
        _client.BaseAddress = new Uri("https://api.mailersend.com/v1/");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var request = new MailerSendEmailRequest(
            new MailerSendEmailContact(message.FromEmail, message.FromName),
            new[] { new MailerSendEmailContact(message.ToEmail) },
            message.Subject,
            BuildTextBody(message),
            message.HtmlBody);

        var response = await _client.PostAsJsonAsync("email", request, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new SendEmailResult(false, Error: string.IsNullOrWhiteSpace(responseBody) ? response.ReasonPhrase : responseBody);

        var messageId = response.Headers.TryGetValues("x-message-id", out var values)
            ? values.FirstOrDefault()
            : null;

        return new SendEmailResult(true, messageId);
    }

    private static string BuildTextBody(ProviderEmailMessage message)
    {
        if (!string.IsNullOrWhiteSpace(message.TextBody))
            return message.TextBody;

        var text = StripHtml(message.HtmlBody);
        return string.IsNullOrWhiteSpace(text) ? message.Subject : text;
    }

    private static string StripHtml(string html)
    {
        var builder = new StringBuilder(html.Length);
        var inTag = false;

        foreach (var character in html)
        {
            if (character == '<')
            {
                inTag = true;
                continue;
            }

            if (character == '>')
            {
                inTag = false;
                builder.Append(' ');
                continue;
            }

            if (!inTag)
                builder.Append(character);
        }

        return WebUtility.HtmlDecode(builder.ToString()).Trim();
    }

    private sealed record MailerSendEmailRequest(
        [property: JsonPropertyName("from")] MailerSendEmailContact From,
        [property: JsonPropertyName("to")] IReadOnlyCollection<MailerSendEmailContact> To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("html")] string Html);

    private sealed record MailerSendEmailContact(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("name")] string? Name = null);
}
