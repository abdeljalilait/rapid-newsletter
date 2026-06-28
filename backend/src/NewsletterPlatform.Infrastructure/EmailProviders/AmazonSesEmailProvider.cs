using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Enums;

namespace NewsletterPlatform.Infrastructure.EmailProviders;

public sealed class AmazonSesEmailProvider : IEmailProvider
{
    private readonly Func<SendEmailRequest, CancellationToken, Task<SendEmailResponse>> _sendEmailAsync;

    public string ProviderName => "Amazon SES";
    public EmailProvider Provider => EmailProvider.AmazonSes;

    public AmazonSesEmailProvider(string accessKeyId, string secretAccessKey, string region)
        : this(CreateClient(accessKeyId, secretAccessKey, region).SendEmailAsync)
    {
    }

    public AmazonSesEmailProvider(Func<SendEmailRequest, CancellationToken, Task<SendEmailResponse>> sendEmailAsync)
    {
        _sendEmailAsync = sendEmailAsync;
    }

    public async Task<SendEmailResult> SendAsync(ProviderEmailMessage message, CancellationToken ct = default)
    {
        var request = new SendEmailRequest
        {
            Source = $"{message.FromName} <{message.FromEmail}>",
            Destination = new Destination
            {
                ToAddresses = new List<string> { message.ToEmail }
            },
            Message = new Message
            {
                Subject = new Content
                {
                    Charset = "UTF-8",
                    Data = message.Subject
                },
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = message.HtmlBody
                    },
                    Text = string.IsNullOrWhiteSpace(message.TextBody)
                        ? null
                        : new Content
                        {
                            Charset = "UTF-8",
                            Data = message.TextBody
                        }
                }
            }
        };

        try
        {
            var response = await _sendEmailAsync(request, ct);
            return response.HttpStatusCode == HttpStatusCode.OK
                ? new SendEmailResult(true, response.MessageId)
                : new SendEmailResult(false, Error: response.HttpStatusCode.ToString());
        }
        catch (AmazonSimpleEmailServiceException ex)
        {
            return new SendEmailResult(false, Error: ex.Message);
        }
    }

    private static AmazonSimpleEmailServiceClient CreateClient(string accessKeyId, string secretAccessKey, string region)
    {
        if (string.IsNullOrWhiteSpace(accessKeyId))
            throw new ArgumentException("Access key ID is required.", nameof(accessKeyId));
        if (string.IsNullOrWhiteSpace(secretAccessKey))
            throw new ArgumentException("Secret access key is required.", nameof(secretAccessKey));
        if (string.IsNullOrWhiteSpace(region))
            throw new ArgumentException("AWS region is required.", nameof(region));

        var credentials = new BasicAWSCredentials(accessKeyId, secretAccessKey);
        return new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.GetBySystemName(region));
    }
}
