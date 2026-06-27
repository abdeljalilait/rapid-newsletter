using System.Security.Claims;
using NewsletterPlatform.Application.Interfaces;

namespace NewsletterPlatform.Api.Middleware;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    private HttpContext? Context => _accessor.HttpContext;

    public Guid? UserId
    {
        get
        {
            var sub = Context?.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
                       ?? Context?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => Context?.User.Identity?.IsAuthenticated ?? false;

    public string? IpAddress => Context?.Connection?.RemoteIpAddress?.ToString();

    public IReadOnlyCollection<string> Roles =>
        Context?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();
}