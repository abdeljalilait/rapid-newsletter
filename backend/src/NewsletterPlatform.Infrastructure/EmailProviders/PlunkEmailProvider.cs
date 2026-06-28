using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class PlunkEmailProvider : IEmailProvider
{
    private readonly HttpClient _client;

    public string ProviderName => "Plunk";
    public EmailProvider Provider => EmailProvider.Plunk;

    public PlunkEmailProvider(HttpClient client, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key is required.", nameof(apiKey));

        _client = client;
        _client.BaseAddress = new Uri("https://next-api.useplunk.com/");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var request = new PlunkSendEmailRequest(message.ToEmail, message.Subject, message.HtmlBody);
        var response = await _client.PostAsJsonAsync("v1/send", request, ct);
        var result = await response.Content.ReadFromJsonAsync<PlunkSendEmailResponse>(ct);

        if (!response.IsSuccessStatusCode || result?.Success != true)
            return new SendEmailResult(false, Error: result?.Error?.Message ?? response.ReasonPhrase);

        return new SendEmailResult(true, result.Data?.Event ?? result.Data?.Contact);
    }

    private sealed record PlunkSendEmailRequest(
        [property: JsonPropertyName("to")] string To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("body")] string Body);

    private sealed record PlunkSendEmailResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("data")] PlunkSendEmailData? Data,
        [property: JsonPropertyName("error")] PlunkError? Error);

    private sealed record PlunkSendEmailData(
        [property: JsonPropertyName("contact")] string? Contact,
        [property: JsonPropertyName("event")] string? Event);

    private sealed record PlunkError(
        [property: JsonPropertyName("message")] string? Message);
}
