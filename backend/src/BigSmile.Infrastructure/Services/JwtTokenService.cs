using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Options;
using BigSmile.SharedKernel.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BigSmile.Infrastructure.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, AuthTokenDescriptor descriptor);
    }

    public sealed record AuthTokenDescriptor(
        string RoleName,
        AccessScope Scope,
        IReadOnlyCollection<string> Permissions,
        Tenant? Tenant = null,
        Branch? CurrentBranch = null);

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string GenerateToken(User user, AuthTokenDescriptor descriptor)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, descriptor.RoleName),
                new Claim(BigSmileClaimTypes.Role, descriptor.RoleName),
                new Claim(BigSmileClaimTypes.Scope, descriptor.Scope.ToClaimValue())
            };

            foreach (var permission in descriptor.Permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(BigSmileClaimTypes.Permission, permission));
            }

            if (descriptor.Tenant != null)
            {
                claims.Add(new Claim(BigSmileClaimTypes.TenantId, descriptor.Tenant.Id.ToString()));
                claims.Add(new Claim(BigSmileClaimTypes.TenantName, descriptor.Tenant.Name));
            }

            if (descriptor.CurrentBranch != null)
            {
                claims.Add(new Claim(BigSmileClaimTypes.BranchId, descriptor.CurrentBranch.Id.ToString()));
                claims.Add(new Claim(BigSmileClaimTypes.BranchName, descriptor.CurrentBranch.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
