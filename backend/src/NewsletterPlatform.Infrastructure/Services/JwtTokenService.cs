using NewsletterPlatform.Application.Common;
using NewsletterPlatform.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NewsletterPlatform.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        var keyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        if (keyBytes.Length < 32)
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 bytes.");
        _key = new SymmetricSecurityKey(keyBytes);
    }

    public TokenPair IssueTokens(Guid userId, string email, IReadOnlyCollection<string> roles, string? createdByIp = null, Guid? replacesTokenId = null)
    {
        var now = DateTime.UtcNow;
        var accessExp = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshExp = now.AddDays(_options.RefreshTokenDays);

        var accessClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("type", "access"),
        };
        accessClaims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var access = CreateToken(accessClaims, accessExp);
        var refreshJti = Guid.NewGuid();
        var refreshClaims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, refreshJti.ToString()),
            new("type", "refresh"),
        };
        var refresh = CreateToken(refreshClaims, refreshExp);

        return new TokenPair(access, refresh, HashRefreshToken(refresh), refreshExp);
    }

    private string CreateToken(IEnumerable<Claim> claims, DateTime expires)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Guid? ValidateAccessToken(string accessToken)
    {
        var principals = ValidateInternal(accessToken, requireType: "access", out _);
        return principals is null ? null : ExtractUserId(principals);
    }

    public (Guid UserId, Guid TokenId)? TryDecodeRefreshToken(string refreshToken)
    {
        var principals = ValidateInternal(refreshToken, requireType: "refresh", out _);
        if (principals is null)
            return null;

        var userId = ExtractUserId(principals);
        var jti = principals.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        if (userId is null || !Guid.TryParse(jti, out var tokenId))
            return null;
        return (userId.Value, tokenId);
    }

    private ClaimsPrincipal? ValidateInternal(string token, string requireType, out DateTime? expires)
    {
        expires = null;
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principals = handler.ValidateToken(token, parameters, out var securityToken);
            var typeClaim = principals.FindFirst("type")?.Value;
            if (typeClaim != requireType)
                return null;
            expires = securityToken.ValidTo;
            return principals;
        }
        catch
        {
            return null;
        }
    }

    private static Guid? ExtractUserId(ClaimsPrincipal principal)
    {
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(bytes);
    }
}