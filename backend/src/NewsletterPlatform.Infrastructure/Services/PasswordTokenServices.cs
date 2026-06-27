using NewsletterPlatform.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace NewsletterPlatform.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return false;
        try { return BCrypt.Net.BCrypt.Verify(password, hash); }
        catch { return false; }
    }
}

public class PasswordTokenGenerator : IPasswordTokenGenerator
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    public string Generate()
    {
        var bytes = new byte[48];
        Rng.GetBytes(bytes);
        return Base64UrlEncode(bytes);
    }

    internal static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
}

public class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string plainToken, string hashedToken)
    {
        if (string.IsNullOrEmpty(hashedToken))
            return false;
        var expected = Hash(plainToken);
        return CryptographicEquals(expected, hashedToken);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return aBytes.Length == bBytes.Length && CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }
}