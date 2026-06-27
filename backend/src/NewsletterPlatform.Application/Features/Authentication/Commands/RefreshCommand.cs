using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;

internal sealed class RefreshHandler : IRequestHandler<RefreshCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _jwtOptions;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUserService _current;

    public RefreshHandler(
        IUserRepository users,
        IUnitOfWork uow,
        IJwtTokenService jwt,
        JwtOptions jwtOptions,
        IAuditLogger audit,
        ICurrentUserService current)
    {
        _users = users;
        _uow = uow;
        _jwt = jwt;
        _jwtOptions = jwtOptions;
        _audit = audit;
        _current = current;
    }

    public async Task<AuthResponse> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var decoded = _jwt.TryDecodeRefreshToken(request.RefreshToken);
        if (decoded is null)
            throw new Exceptions.UnauthorizedException("Invalid refresh token.");

        var tokenHash = _jwt.HashRefreshToken(request.RefreshToken);
        var stored = await _users.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);

        if (stored is null)
            throw new Exceptions.UnauthorizedException("Invalid refresh token.");

        if (stored.Status != Domain.Enums.RefreshTokenStatus.Active || DateTime.UtcNow >= stored.ExpiresAt)
        {
            await _users.RevokeAllActiveTokensForUserAsync(stored.UserId, "Possible token reuse detected", cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            await _audit.LogAsync(null, stored.UserId, "auth.token_reuse_detected", "RefreshToken", stored.Id, null, _current.IpAddress, cancellationToken);
            throw new Exceptions.UnauthorizedException("Refresh token is no longer active. Please sign in again.");
        }

        var user = await _users.GetByIdWithTokensAsync(stored.UserId, cancellationToken);
        if (user is null)
            throw new Exceptions.UnauthorizedException("Invalid refresh token.");

        stored.Status = Domain.Enums.RefreshTokenStatus.Used;
        stored.RevokedAt = DateTime.UtcNow;
        stored.UpdatedAt = DateTime.UtcNow;

        var roles = new[] { "User" };
        var tokens = _jwt.IssueTokens(user.Id, user.Email, roles, _current.IpAddress, replacesTokenId: stored.Id);
        var newToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.RefreshTokenHash,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedByIp = _current.IpAddress,
            ReplacesTokenId = stored.Id,
            Status = Domain.Enums.RefreshTokenStatus.Active
        };
        user.RefreshTokens.Add(newToken);

        await _uow.SaveChangesAsync(cancellationToken);

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
