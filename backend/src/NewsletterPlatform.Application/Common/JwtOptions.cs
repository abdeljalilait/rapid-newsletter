namespace NewsletterPlatform.Application.Common;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 15;
    public int RefreshTokenDays { get; set; } = 30;
    public bool DevAutoConfirmEmail { get; set; } = false;
    public bool DevReturnTokens { get; set; } = false;
}

public class AuthOptions
{
    public const string SectionName = "Auth";
    public int MinPasswordLength { get; set; } = 8;
    public int MaxPasswordLength { get; set; } = 128;
    public TimeSpan PasswordResetLifetime { get; set; } = TimeSpan.FromHours(1);
}