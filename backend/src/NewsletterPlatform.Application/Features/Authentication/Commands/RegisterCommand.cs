using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using NewsletterPlatform.Domain.Entities;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record RegisterCommand(string Email, string Password, string FirstName, string? LastName)
    : IRequest<AuthResponse>;

internal sealed class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly JwtOptions _jwtOptions;
    private readonly IAuditLogger _audit;
    private readonly ICurrentUserService _current;
    private readonly IPasswordTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailSender _emailSender;

    public RegisterHandler(
        IUserRepository users,
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        JwtOptions jwtOptions,
        IAuditLogger audit,
        ICurrentUserService current,
        IPasswordTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher,
        IEmailSender emailSender)
    {
        _users = users;
        _uow = uow;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _jwtOptions = jwtOptions;
        _audit = audit;
        _current = current;
        _tokenGenerator = tokenGenerator;
        _tokenHasher = tokenHasher;
        _emailSender = emailSender;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();

        if (await _users.ExistsByEmailAsync(normalized, cancellationToken))
            throw new Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Email", "An account with this email already exists."),
            });

        var user = new User
        {
            Email = normalized,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FirstName = request.FirstName?.Trim() ?? string.Empty,
            LastName = request.LastName?.Trim()
        };

        if (_jwtOptions.DevAutoConfirmEmail)
        {
            user.EmailConfirmedAt = DateTime.UtcNow;
            user.EmailConfirmationTokenHash = null;
            user.EmailConfirmationTokenExpiresAt = null;
        }
        else
        {
            var token = _tokenGenerator.Generate();
            user.EmailConfirmationTokenHash = _tokenHasher.Hash(token);
            user.EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
            await _emailSender.SendAsync(
                user.Email,
                "Confirm your email",
                $"<p>Confirm your email with this token: <code>{token}</code></p>",
                $"Confirm token: {token}",
                cancellationToken);
        }

        await _users.AddAsync(user, cancellationToken);

        var roles = new[] { "User" };
        var tokens = _jwt.IssueTokens(user.Id, user.Email, roles, _current.IpAddress);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.RefreshTokenHash,
            ExpiresAt = tokens.RefreshTokenExpiresAt,
            CreatedByIp = _current.IpAddress,
            Status = Domain.Enums.RefreshTokenStatus.Active
        };
        user.RefreshTokens.Add(refreshToken);

        await _uow.SaveChangesAsync(cancellationToken);

        await _audit.LogAsync(null, user.Id, "user.registered", "User", user.Id, null, _current.IpAddress, cancellationToken);

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
