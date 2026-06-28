using System.Net;
using Amazon.SimpleEmail.Model;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Infrastructure.EmailProviders;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class AmazonSesEmailProviderTests
{
    [Fact]
    public async Task SendAsync_SendsEmailPayloadThroughSesClient()
    {
        SendEmailRequest? capturedRequest = null;
        var provider = new AmazonSesEmailProvider((request, _) =>
        {
            capturedRequest = request;
            return Task.FromResult(new SendEmailResponse
            {
                HttpStatusCode = HttpStatusCode.OK,
                MessageId = "ses-message-id"
            });
        });

        var result = await provider.SendAsync(new ProviderEmailMessage(
            "from@example.com",
            "From Name",
            "to@example.com",
            "Hello",
            "<p>Hello</p>",
            "Plain hello"));

        Assert.True(result.Success);
        Assert.Equal("ses-message-id", result.ProviderMessageId);

        var request = capturedRequest!;
        Assert.Equal("From Name <from@example.com>", request.Source);
        Assert.Equal("to@example.com", Assert.Single(request.Destination.ToAddresses));
        Assert.Equal("Hello", request.Message.Subject.Data);
        Assert.Equal("UTF-8", request.Message.Subject.Charset);
        Assert.Equal("<p>Hello</p>", request.Message.Body.Html.Data);
        Assert.Equal("Plain hello", request.Message.Body.Text.Data);
    }
}
