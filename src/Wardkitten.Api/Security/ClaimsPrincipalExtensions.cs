using System.Security.Claims;

namespace Wardkitten.Api.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id del usuario autenticado (claim "sub"). JwtBearer se configura con MapInboundClaims=false.</summary>
    public static string? UserId(this ClaimsPrincipal principal)
        => principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
}
