using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MicroLendingSystem.Backend.Infrastructure;

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor _http = httpContextAccessor;

    public int? UserId
    {
        get
        {
            var principal = _http.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? principal.FindFirstValue("sub");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public bool IsAdmin
    {
        get
        {
            var principal = _http.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            if (principal.IsInRole("Admin"))
            {
                return true;
            }

            static bool IsAdminClaim(string? v) =>
                string.Equals(v, "Admin", StringComparison.OrdinalIgnoreCase);

            return principal.Claims.Any(c =>
                (c.Type == ClaimTypes.Role || c.Type == "role") && IsAdminClaim(c.Value));
        }
    }

    // CurrentUserAccessor � same pattern as IsAdmin
    public bool IsBorrower
    {
        get
        {
            var p = _http.HttpContext?.User;
            if (p?.Identity?.IsAuthenticated != true) return false;
            if (p.IsInRole("Borrower")) return true;
            return p.Claims.Any(c =>
                (c.Type == ClaimTypes.Role || c.Type == "role") &&
                string.Equals(c.Value, "Borrower", StringComparison.OrdinalIgnoreCase));
        }
    }
    public bool TryGetUserId(out int userId)
    {
        userId = 0;
        var principal = _http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                  ?? principal.FindFirstValue("sub");
        return !string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out userId);
    }
}
