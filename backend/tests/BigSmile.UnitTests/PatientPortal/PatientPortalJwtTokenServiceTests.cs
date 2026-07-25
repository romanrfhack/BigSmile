using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalJwtTokenServiceTests
    {
        [Fact]
        public void Generate_UsesPatientOnlyClaimsAndSeparateAudience()
        {
            var tenantId = Guid.NewGuid();
            var patient = new Patient(
                tenantId,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "ana.portal",
                "versioned-password-hash",
                new DateTime(2026, 7, 25, 8, 0, 0, DateTimeKind.Utc));
            var settings = new FixedJwtSettings();
            var service = new PatientPortalJwtTokenService(settings);
            var issuedAtUtc = new DateTime(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc);

            var generated = service.Generate(account, issuedAtUtc);
            var token = new JwtSecurityTokenHandler().ReadJwtToken(generated.Token);

            Assert.Equal(settings.Issuer, token.Issuer);
            Assert.Contains(settings.Audience, token.Audiences);
            Assert.Equal(issuedAtUtc.AddHours(1), generated.ExpiresAtUtc);
            Assert.Equal(account.Id.ToString(), token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(tenantId.ToString(), token.Claims.Single(claim => claim.Type == BigSmileClaimTypes.TenantId).Value);
            Assert.Equal(patient.Id.ToString(), token.Claims.Single(claim => claim.Type == BigSmileClaimTypes.PatientId).Value);
            Assert.Equal("patient", token.Claims.Single(claim => claim.Type == BigSmileClaimTypes.Scope).Value);
            Assert.Equal("1", token.Claims.Single(claim => claim.Type == BigSmileClaimTypes.SessionVersion).Value);
            Assert.Contains(token.Claims, claim => claim.Type == JwtRegisteredClaimNames.Jti);

            Assert.DoesNotContain(token.Claims, claim => claim.Type is ClaimTypes.Role or BigSmileClaimTypes.Role);
            Assert.DoesNotContain(token.Claims, claim => claim.Type == BigSmileClaimTypes.Permission);
            Assert.DoesNotContain(token.Claims, claim => claim.Type is BigSmileClaimTypes.BranchId or BigSmileClaimTypes.BranchName);
        }

        private sealed class FixedJwtSettings : IPatientPortalJwtSettings
        {
            public string Secret => "patient-portal-test-secret-that-is-distinct-and-long-enough";
            public string Issuer => "BigSmile.PatientPortal.Tests";
            public string Audience => "BigSmile.PatientPortal.Tests";
            public TimeSpan AccessTokenLifetime => TimeSpan.FromHours(1);
        }
    }
}
