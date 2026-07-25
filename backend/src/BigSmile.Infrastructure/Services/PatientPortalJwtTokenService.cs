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
            EnsureUtc(issuedAtUtc);

            if (!account.IsActive ||
                !account.PatientId.HasValue ||
                account.PatientId.Value == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient access token requires an active account linked to a patient.");
            }

            return GenerateToken(
                account,
                issuedAtUtc,
                new[]
                {
                    new Claim(BigSmileClaimTypes.PatientId, account.PatientId.Value.ToString()),
                    new Claim(BigSmileClaimTypes.Scope, AccessScope.Patient.ToClaimValue())
                });
        }

        public GeneratedPatientPortalAccessToken GenerateForIntake(
            PatientPortalAccount account,
            PatientIntake intake,
            DateTime issuedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(intake);
            EnsureUtc(issuedAtUtc);

            if (!account.IsActive || account.PatientId.HasValue)
            {
                throw new InvalidOperationException(
                    "Intake-only access token requires an active unlinked patient portal account.");
            }

            if (intake.TenantId != account.TenantId ||
                intake.PatientPortalAccountId != account.Id ||
                intake.PatientId.HasValue ||
                intake.Origin != PatientIntakeOrigin.NewPatientWaitingRoom ||
                intake.Status != PatientIntakeStatus.Draft ||
                intake.IsExpiredAt(issuedAtUtc))
            {
                throw new InvalidOperationException(
                    "Intake-only access token requires the active waiting-room draft owned by the account.");
            }

            return GenerateToken(
                account,
                issuedAtUtc,
                new[]
                {
                    new Claim(BigSmileClaimTypes.IntakeId, intake.Id.ToString()),
                    new Claim(BigSmileClaimTypes.Scope, AccessScope.PatientIntake.ToClaimValue())
                });
        }

        private GeneratedPatientPortalAccessToken GenerateToken(
            PatientPortalAccount account,
            DateTime issuedAtUtc,
            IReadOnlyCollection<Claim> scopeClaims)
        {
            var expiresAtUtc = issuedAtUtc.Add(_settings.AccessTokenLifetime);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(BigSmileClaimTypes.TenantId, account.TenantId.ToString()),
                new(BigSmileClaimTypes.SessionVersion, account.SessionVersion.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };
            claims.AddRange(scopeClaims);

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_settings.Secret));
            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);
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

        private static void EnsureUtc(DateTime issuedAtUtc)
        {
            if (issuedAtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient access token issue time must be UTC.",
                    nameof(issuedAtUtc));
            }
        }
    }
}
