using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using NewsletterPlatform.Application.Interfaces;

namespace NewsletterPlatform.Infrastructure.Services;

public sealed class AesSecretProtector : ISecretProtector
{
    private readonly byte[] _key;

    public AesSecretProtector(IConfiguration configuration)
    {
        var configured = configuration["Security:SecretEncryptionKey"];
        if (string.IsNullOrWhiteSpace(configured))
            throw new InvalidOperationException("Security:SecretEncryptionKey is required.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(configured));
    }

    public string Protect(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new ArgumentException("Secret is required.", nameof(secret));

        var nonce = RandomNumberGenerator.GetBytes(12);
        var plain = Encoding.UTF8.GetBytes(secret);
        var cipher = new byte[plain.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plain, cipher, tag);

        return Convert.ToBase64String(nonce.Concat(tag).Concat(cipher).ToArray());
    }

    public string Unprotect(string protectedSecret)
    {
        var bytes = Convert.FromBase64String(protectedSecret);
        var nonce = bytes[..12];
        var tag = bytes[12..28];
        var cipher = bytes[28..];
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }
}
