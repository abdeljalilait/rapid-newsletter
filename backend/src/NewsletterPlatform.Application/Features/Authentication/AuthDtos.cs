namespace NewsletterPlatform.Application.Features.Authentication;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string? LastName,
    bool EmailConfirmed,
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    TimeSpan AccessTokenExpiresIn);

public sealed record RegisterRequest(string Email, string Password, string FirstName, string? LastName);

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);

public sealed record RequestPasswordResetRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record ConfirmEmailRequest(string Token);