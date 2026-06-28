using System.Net;
using System.Text.Json;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Infrastructure.EmailProviders;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class MailerSendEmailProviderTests
{
    [Fact]
    public async Task SendAsync_PostsEmailPayloadToMailerSend()
    {
        var handler = new CapturingHandler();
        var provider = new MailerSendEmailProvider(new HttpClient(handler), "test-key");

        var result = await provider.SendAsync(new ProviderEmailMessage(
            "from@example.com",
            "From Name",
            "to@example.com",
            "Hello",
            "<h1>Hello</h1>",
            "Plain hello"));

        Assert.True(result.Success);
        Assert.Equal("msg_123", result.ProviderMessageId);
        Assert.Equal("https://api.mailersend.com/v1/email", handler.RequestUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("test-key", handler.AuthorizationParameter);

        using var body = JsonDocument.Parse(handler.Body!);
        var root = body.RootElement;
        Assert.Equal("from@example.com", root.GetProperty("from").GetProperty("email").GetString());
        Assert.Equal("From Name", root.GetProperty("from").GetProperty("name").GetString());
        Assert.Equal("to@example.com", root.GetProperty("to")[0].GetProperty("email").GetString());
        Assert.Equal("Hello", root.GetProperty("subject").GetString());
        Assert.Equal("Plain hello", root.GetProperty("text").GetString());
        Assert.Equal("<h1>Hello</h1>", root.GetProperty("html").GetString());
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? RequestUri { get; private set; }
        public string? AuthorizationScheme { get; private set; }
        public string? AuthorizationParameter { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri?.ToString();
            AuthorizationScheme = request.Headers.Authorization?.Scheme;
            AuthorizationParameter = request.Headers.Authorization?.Parameter;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);

            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            response.Headers.Add("x-message-id", "msg_123");
            return response;
        }
    }
}
