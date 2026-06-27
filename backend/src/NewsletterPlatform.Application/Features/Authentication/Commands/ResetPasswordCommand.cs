using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : IRequest;

internal sealed class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenHasher _tokenHasher;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUserService _current;

    public ResetPasswordHandler(IUserRepository users, IUnitOfWork uow, IPasswordHasher passwordHasher, ITokenHasher tokenHasher, IAuditLogger audit, ICurrentUserService current)
    {
        _users = users; _uow = uow; _passwordHasher = passwordHasher; _tokenHasher = tokenHasher; _audit = audit; _current = current;
    }

    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHasher.Hash(request.Token);
        var user = await _users.GetByPasswordResetTokenAsync(tokenHash, cancellationToken);

        if (user is null)
            throw new Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Token", "Password reset token is invalid or expired."),
            });

        if (user.PasswordResetTokenHash is null
            || user.PasswordResetTokenExpiresAt is null
            || DateTime.UtcNow > user.PasswordResetTokenExpiresAt.Value
            || !string.Equals(user.PasswordResetTokenHash, tokenHash, StringComparison.Ordinal))
            throw new Exceptions.UnauthorizedException("Password reset token is invalid or expired.");

        await _users.RevokeAllActiveTokensForUserAsync(user.Id, "password_reset", cancellationToken);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _uow.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(null, user.Id, "user.password_reset", "User", user.Id, null, _current.IpAddress, cancellationToken);
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(AuthOptions auth)
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(auth.MinPasswordLength)
            .WithMessage($"Password must be at least {auth.MinPasswordLength} characters.")
            .MaximumLength(auth.MaxPasswordLength);
    }
}
