using System.Net;
using System.Text.Json;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Infrastructure.EmailProviders;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class SenderEmailProviderTests
{
    [Fact]
    public async Task SendAsync_ReturnsSuccess_WhenApiReturnsEmailId()
    {
        var handler = new FakeHandler(req =>
        {
            Assert.Equal("https://api.sender.net/v2/message/send", req.RequestUri?.ToString());
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { success = true, emailId = "abc123" }))
            };
            return Task.FromResult(response);
        });

        var provider = new SenderEmailProvider(new HttpClient(handler), "test-key");
        var result = await provider.SendAsync(new ProviderEmailMessage(
            "from@example.com", "From", "to@example.com", "Subject", "<p>Hi</p>", "Hi"));

        Assert.True(result.Success);
        Assert.Equal("abc123", result.ProviderMessageId);
    }

    [Fact]
    public async Task SendAsync_ReturnsFailure_WhenApiReturnsError()
    {
        var handler = new FakeHandler(_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"success\":false,\"message\":\"Invalid request\"}")
        }));

        var provider = new SenderEmailProvider(new HttpClient(handler), "test-key");
        var result = await provider.SendAsync(new ProviderEmailMessage(
            "from@example.com", "From", "to@example.com", "Subject", "<p>Hi</p>"));

        Assert.False(result.Success);
        Assert.Contains("Invalid request", result.Error);
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _responder;

        public FakeHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) => _responder = responder;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _responder(request);
    }
}
