using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using NewsletterPlatform.Domain.Enums;
using NewsletterPlatform.Infrastructure.EmailProviders;
using NewsletterPlatform.Infrastructure.Services;
using Xunit;

namespace NewsletterPlatform.Tests;

public sealed class EmailProviderFactoryTests
{
    [Theory]
    [InlineData(EmailProvider.Resend, typeof(ResendEmailProvider))]
    [InlineData(EmailProvider.MailerSend, typeof(MailerSendEmailProvider))]
    [InlineData(EmailProvider.Plunk, typeof(PlunkEmailProvider))]
    [InlineData(EmailProvider.AmazonSes, typeof(AmazonSesEmailProvider))]
    [InlineData(EmailProvider.Mailtrap, typeof(DevEmailProvider))]
    public void Create_ReturnsCorrectProviderType(EmailProvider provider, Type expectedType)
    {
        var factory = CreateFactory();
        var account = new EmailProviderAccount
        {
            Provider = provider,
            EncryptedApiKey = "protected:key",
            EncryptedApiSecret = provider == EmailProvider.AmazonSes ? "protected:secret" : string.Empty,
            SendingDomain = provider == EmailProvider.AmazonSes ? "us-east-1" : null
        };

        var instance = factory.Create(account);

        Assert.IsType(expectedType, instance);
    }

    [Fact]
    public void Create_Throws_WhenProviderIsNotSupported()
    {
        var factory = CreateFactory();
        var account = new EmailProviderAccount
        {
            Provider = (EmailProvider)999,
            EncryptedApiKey = "protected:key"
        };

        Assert.Throws<NotSupportedException>(() => factory.Create(account));
    }

    private static EmailProviderFactory CreateFactory() =>
        new(new TestHttpClientFactory(), new TestSecretProtector(), new Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory());

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    private sealed class TestSecretProtector : ISecretProtector
    {
        public string Protect(string secret) => $"protected:{secret}";
        public string Unprotect(string protectedSecret) => protectedSecret["protected:".Length..];
    }
}
