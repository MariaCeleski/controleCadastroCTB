using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccessControl.Application.Security;
using Microsoft.IdentityModel.Tokens;

namespace AccessControl.Infrastructure.Services;

public sealed class JwtTokenService(string issuer, string audience, string signingKey) : ITokenService
{
    public AuthenticatedSession Create(Guid userId, Guid organizationId, string name, string role)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(30);
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, [new(ClaimTypes.NameIdentifier, userId.ToString()), new("organization_id", organizationId.ToString()), new(ClaimTypes.Name, name), new(ClaimTypes.Role, role)], expires: expiresAt.UtcDateTime, signingCredentials: credentials);
        return new AuthenticatedSession(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, name, role);
    }
}
