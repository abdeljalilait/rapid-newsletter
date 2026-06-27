using NewsletterPlatform.Application.Interfaces;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record LogoutCommand(string RefreshToken) : IRequest;

internal sealed class LogoutHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IJwtTokenService _jwt;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUserService _current;

    public LogoutHandler(IUserRepository users, IUnitOfWork uow, IJwtTokenService jwt, IAuditLogger audit, ICurrentUserService current)
    {
        _users = users;
        _uow = uow;
        _jwt = jwt;
        _audit = audit;
        _current = current;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return;

        var tokenHash = _jwt.HashRefreshToken(request.RefreshToken);
        var stored = await _users.GetRefreshTokenByHashAsync(tokenHash, cancellationToken);
        if (stored is null || stored.Status != Domain.Enums.RefreshTokenStatus.Active || DateTime.UtcNow >= stored.ExpiresAt)
            return;

        stored.Status = Domain.Enums.RefreshTokenStatus.Revoked;
        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedReason = "user_logout";
        stored.UpdatedAt = DateTime.UtcNow;
        var user = await _users.GetByIdAsync(stored.UserId, cancellationToken)
            ?? throw new Exceptions.UnauthorizedException("Invalid refresh token.");

        await _uow.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(null, stored.UserId, "user.logged_out", "RefreshToken", stored.Id, null, _current.IpAddress, cancellationToken);
    }
}
