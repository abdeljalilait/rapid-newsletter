using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Infrastructure.EmailProviders;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class PlunkEmailProviderTests
{
    [Fact]
    public async Task SendAsync_PostsEmailPayloadToPlunk()
    {
        var handler = new CapturingHandler();
        var provider = new PlunkEmailProvider(new HttpClient(handler), "sk_test");

        var result = await provider.SendAsync(new ProviderEmailMessage(
            "from@example.com",
            "From Name",
            "to@example.com",
            "Hello",
            "<p>Your message here</p>",
            "Plain text"));

        Assert.True(result.Success);
        Assert.Equal("evt_xyz789", result.ProviderMessageId);
        Assert.Equal("https://next-api.useplunk.com/v1/send", handler.RequestUri);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("sk_test", handler.AuthorizationParameter);

        using var body = JsonDocument.Parse(handler.Body!);
        var root = body.RootElement;
        Assert.Equal("to@example.com", root.GetProperty("to").GetString());
        Assert.Equal("Hello", root.GetProperty("subject").GetString());
        Assert.Equal("<p>Your message here</p>", root.GetProperty("body").GetString());
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new
                {
                    success = true,
                    data = new
                    {
                        contact = "cnt_abc123",
                        @event = "evt_xyz789"
                    }
                })
            };
        }
    }
}
