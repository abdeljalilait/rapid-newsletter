using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class LoopsEmailProvider : IEmailProvider
{
    private readonly HttpClient _client;
    private readonly string _transactionalId;

    public string ProviderName => "Loops";
    public EmailProvider Provider => EmailProvider.Loops;

    public LoopsEmailProvider(HttpClient client, string apiKey, string transactionalId)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));
        if (string.IsNullOrWhiteSpace(transactionalId))
            throw new ArgumentException("Loops transactional email ID is required and should be stored in SendingDomain.", nameof(transactionalId));

        _client = client;
        _transactionalId = transactionalId;
        _client.BaseAddress = new Uri("https://app.loops.so/api/v1/");
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var payload = new
        {
            email = message.ToEmail,
            transactionalId = _transactionalId,
            dataVariables = new Dictionary<string, object>
            {
                ["subject"] = message.Subject,
                ["htmlBody"] = message.HtmlBody,
                ["textBody"] = message.TextBody ?? string.Empty,
                ["fromName"] = message.FromName,
                ["fromEmail"] = message.FromEmail
            }
        };

        var response = await _client.PostAsJsonAsync("transactional", payload, ct);
        var content = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new SendEmailResult(false, Error: content);

        var result = await response.Content.ReadFromJsonAsync<LoopsSendResponse>(ct);
        return result?.Success == true
            ? new SendEmailResult(true)
            : new SendEmailResult(false, Error: result?.Message ?? content);
    }

    private sealed class LoopsSendResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
