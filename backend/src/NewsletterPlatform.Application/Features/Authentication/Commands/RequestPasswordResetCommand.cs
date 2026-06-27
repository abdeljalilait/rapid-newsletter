using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using MediatR;

namespace NewsletterPlatform.Application.Features.Authentication.Commands;

public sealed record RequestPasswordResetCommand(string Email) : IRequest<PasswordResetResult>;

public sealed record PasswordResetResult(bool Sent, string? DevToken);

internal sealed class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, PasswordResetResult>
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _uow;
    private readonly IPasswordTokenGenerator _tokenGenerator;
    private readonly ITokenHasher _tokenHasher;
    private readonly IEmailSender _emailSender;
    private readonly AuthOptions _auth;
    private readonly JwtOptions _jwt;

    public RequestPasswordResetHandler(
        IUserRepository users, IUnitOfWork uow, IPasswordTokenGenerator tokenGenerator,
        ITokenHasher tokenHasher, IEmailSender emailSender, AuthOptions auth, JwtOptions jwt)
    {
        _users = users; _uow = uow; _tokenGenerator = tokenGenerator; _tokenHasher = tokenHasher;
        _emailSender = emailSender; _auth = auth; _jwt = jwt;
    }

    public async Task<PasswordResetResult> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var normalized = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(normalized, cancellationToken);
        if (user is null)
            return new PasswordResetResult(true, null);

        var token = _tokenGenerator.Generate();
        user.PasswordResetTokenHash = _tokenHasher.Hash(token);
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.Add(_auth.PasswordResetLifetime);
        user.UpdatedAt = DateTime.UtcNow;
        await _uow.SaveChangesAsync(cancellationToken);

        await _emailSender.SendAsync(
            user.Email,
            "Reset your password",
            $"<p>Use this token to reset your password: <code>{token}</code></p>",
            $"Reset token: {token}",
            cancellationToken);

        return new PasswordResetResult(true, _jwt.DevReturnTokens ? token : null);
    }
}
