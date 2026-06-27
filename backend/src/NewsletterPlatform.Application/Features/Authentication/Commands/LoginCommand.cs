using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

internal sealed class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _jwtOptions;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUserService _current;

    public LoginHandler(
        IUserRepository users,
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        JwtOptions jwtOptions,
        IAuditLogger audit,
        ICurrentUserService current)
    {
        _users = users;
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _jwtOptions = jwtOptions;
        _audit = audit;
        _current = current;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailWithTokensAsync(normalized, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exceptions.UnauthorizedException("Invalid email or password.");

        if (!user.EmailConfirmedAt.HasValue)
            throw new Exceptions.UnauthorizedException("Please confirm your email before signing in.");

        var roles = new[] { "User" };
        var tokens = _jwt.IssueTokens(user.Id, user.Email, roles, _current.IpAddress);
        var refreshToken = new Domain.Entities.RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.RefreshTokenHash,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedByIp = _current.IpAddress,
            Status = Domain.Enums.RefreshTokenStatus.Active
        };
        user.RefreshTokens.Add(refreshToken);

        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(null, user.Id, "user.logged_in", "User", user.Id, null, _current.IpAddress, cancellationToken);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.EmailConfirmedAt.HasValue,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.RefreshTokenExpiresAt,
            TimeSpan.FromMinutes(_jwtOptions.AccessTokenMinutes));
    }
}
