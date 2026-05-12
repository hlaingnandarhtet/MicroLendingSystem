using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MicroLendingSystem.Database.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MicroLendingSystem.Backend.Options;

namespace MicroLendingSystem.Backend.Infrastructure;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public string CreateAccessToken(User user, IEnumerable<string> permissions)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_jwt.Secret);
        if (keyBytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes (UTF-8).");
        }

        var issuer = string.IsNullOrWhiteSpace(_jwt.Issuer) ? "micro-lending" : _jwt.Issuer;
        var audience = string.IsNullOrWhiteSpace(_jwt.Audience) ? "micro-lending-api" : _jwt.Audience;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.Name)
        };

        // Add Permission claims
        foreach (var p in permissions)
        {
            claims.Add(new Claim("Permission", p));
        }

        var signingKey = new SymmetricSecurityKey(keyBytes);
        var signingCreds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
            signingCredentials: signingCreds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
