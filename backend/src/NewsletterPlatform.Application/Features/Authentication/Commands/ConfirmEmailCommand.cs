using NewsletterPlatform.Application.Interfaces;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record ConfirmEmailCommand(string Token) : IRequest<ConfirmEmailResult>;
public sealed record ConfirmEmailResult(bool Confirmed, bool AlreadyConfirmed);

internal sealed class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUserService _current;

    public ConfirmEmailHandler(IUserRepository users, IUnitOfWork uow, ITokenHasher tokenHasher, IAuditLogger audit, ICurrentUserService current)
    {
        _users = users; _uow = uow; _tokenHasher = tokenHasher; _audit = audit; _current = current;
    }

    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHasher.Hash(request.Token);
        var user = await _users.GetByEmailConfirmationTokenAsync(tokenHash, cancellationToken);

        if (user is null)
            throw new Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Token", "Email confirmation token is invalid or expired."),
            });

        if (user.EmailConfirmedAt.HasValue)
            return new ConfirmEmailResult(true, true);

        if (user.EmailConfirmationTokenHash is null
            || user.EmailConfirmationTokenExpiresAt is null
            || DateTime.UtcNow > user.EmailConfirmationTokenExpiresAt.Value
            || !string.Equals(user.EmailConfirmationTokenHash, tokenHash, StringComparison.Ordinal))
            throw new Exceptions.UnauthorizedException("Email confirmation token is invalid or expired.");

        user.EmailConfirmedAt = DateTime.UtcNow;
        user.EmailConfirmationTokenHash = null;
        user.EmailConfirmationTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(null, user.Id, "user.email_confirmed", "User", user.Id, null, _current.IpAddress, cancellationToken);

        return new ConfirmEmailResult(true, false);
    }
}
