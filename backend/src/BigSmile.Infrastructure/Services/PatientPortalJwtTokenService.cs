using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalJwtTokenService : IPatientPortalJwtTokenService
    {
        private readonly IPatientPortalJwtSettings _settings;

        public PatientPortalJwtTokenService(IPatientPortalJwtSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public GeneratedPatientPortalAccessToken Generate(
            PatientPortalAccount account,
            DateTime issuedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(account);

            if (issuedAtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Patient access token issue time must be UTC.", nameof(issuedAtUtc));
            }

            if (!account.IsActive || !account.PatientId.HasValue || account.PatientId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient access token requires an active account linked to a patient.");
            }

            var expiresAtUtc = issuedAtUtc.Add(_settings.AccessTokenLifetime);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(BigSmileClaimTypes.TenantId, account.TenantId.ToString()),
                new Claim(BigSmileClaimTypes.PatientId, account.PatientId.Value.ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.Patient.ToClaimValue()),
                new Claim(BigSmileClaimTypes.SessionVersion, account.SessionVersion.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: issuedAtUtc,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new GeneratedPatientPortalAccessToken(
                new JwtSecurityTokenHandler().WriteToken(token),
                expiresAtUtc);
        }
    }
}
