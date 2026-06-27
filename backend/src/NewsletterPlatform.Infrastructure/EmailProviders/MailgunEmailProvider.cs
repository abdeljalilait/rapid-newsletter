using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class MailgunEmailProvider : IEmailProvider
{
    private readonly HttpClient _client;
    private readonly string _domain;

    public string ProviderName => "Mailgun";
    public EmailProvider Provider => EmailProvider.Mailgun;

    public MailgunEmailProvider(HttpClient client, string apiKey, string domain)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain is required.", nameof(domain));

        _client = client;
        _domain = domain;
        _client.BaseAddress = new Uri($"https://api.mailgun.net/v3/{domain}/");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{apiKey}")));
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent($"{message.FromName} <{message.FromEmail}>"), "from" },
            { new StringContent(message.ToEmail), "to" },
            { new StringContent(message.Subject), "subject" },
            { new StringContent(message.HtmlBody), "html" }
        };

        if (!string.IsNullOrEmpty(message.TextBody))
            content.Add(new StringContent(message.TextBody), "text");

        var response = await _client.PostAsync("messages", content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new SendEmailResult(false, Error: responseBody);

        var result = JsonSerializer.Deserialize<MailgunSendResponse>(responseBody);
        return new SendEmailResult(true, result?.Id?.Trim('<', '>'));
    }

    private sealed class MailgunSendResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
