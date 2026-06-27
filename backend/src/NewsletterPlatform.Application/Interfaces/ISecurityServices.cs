namespace NewsletterPlatform.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IPasswordTokenGenerator
{
    string Generate();
}

public interface ITokenHasher
{
    string Hash(string token);
    bool Verify(string plainToken, string hashedToken);
}

public sealed record TokenPair(string AccessToken, string RefreshToken, string RefreshTokenHash, DateTime RefreshTokenExpiresAt);

public interface IJwtTokenService
{
    TokenPair IssueTokens(Guid userId, string email, IReadOnlyCollection<string> roles, string? createdByIp = null, Guid? replacesTokenId = null);
    Guid? ValidateAccessToken(string accessToken);
    (Guid UserId, Guid TokenId)? TryDecodeRefreshToken(string refreshToken);
    string HashRefreshToken(string refreshToken);
}

public interface ISecretProtector
{
    string Protect(string secret);
    string Unprotect(string protectedSecret);
}
