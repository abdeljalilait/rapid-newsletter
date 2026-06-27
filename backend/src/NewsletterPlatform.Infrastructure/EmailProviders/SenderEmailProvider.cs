using System.Net.Http.Json;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class SenderEmailProvider : IEmailProvider
{
    private readonly HttpClient _client;

    public string ProviderName => "Sender";
    public EmailProvider Provider => EmailProvider.Sender;

    public SenderEmailProvider(HttpClient client, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));

        _client = client;
        _client.BaseAddress = new Uri("https://api.sender.net/v2/");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var payload = new
        {
            from = new { email = message.FromEmail, name = message.FromName },
            to = new { email = message.ToEmail },
            subject = message.Subject,
            html = message.HtmlBody,
            text = message.TextBody
        };

        var response = await _client.PostAsJsonAsync("message/send", payload, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new SendEmailResult(false, Error: content);

        var result = await response.Content.ReadFromJsonAsync<SenderSendResponse>(ct);
        return result?.Success == true
            ? new SendEmailResult(true, result.EmailId)
            : new SendEmailResult(false, Error: result?.Message ?? content);
    }

    private sealed class SenderSendResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? EmailId { get; set; }
    }
}
