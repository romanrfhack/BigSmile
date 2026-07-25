using System.IdentityModel.Tokens.Jwt;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Services;
using BigSmile.Application.Interfaces.Security;
using BigSmile.SharedKernel.Authorization;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeJwtTokenServiceTests
    {
        private static readonly DateTime IssuedAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void GenerateForIntake_UsesOnlyIntakeScopedClaims()
        {
            var tenantId = Guid.NewGuid();
            var account = PatientPortalAccount.CreateUnlinked(
                tenantId,
                "waiting.patient",
                "versioned-password-hash",
                IssuedAtUtc);
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch: null,
                IssuedAtUtc);
            var service = new PatientPortalJwtTokenService(new FixedJwtSettings());

            var generated = service.GenerateForIntake(
                account,
                intake,
                IssuedAtUtc);
            var token = new JwtSecurityTokenHandler().ReadJwtToken(generated.Token);
            var claims = token.Claims.ToLookup(claim => claim.Type, claim => claim.Value);

            Assert.Contains(account.Id.ToString(), claims[JwtRegisteredClaimNames.Sub]);
            Assert.Contains(tenantId.ToString(), claims[BigSmileClaimTypes.TenantId]);
            Assert.Contains(intake.Id.ToString(), claims[BigSmileClaimTypes.IntakeId]);
            Assert.Contains(
                AccessScope.PatientIntake.ToClaimValue(),
                claims[BigSmileClaimTypes.Scope]);
            Assert.Contains(
                account.SessionVersion.ToString(),
                claims[BigSmileClaimTypes.SessionVersion]);
            Assert.NotEmpty(claims[JwtRegisteredClaimNames.Jti]);
            Assert.Empty(claims[BigSmileClaimTypes.PatientId]);
            Assert.Empty(claims[BigSmileClaimTypes.Permission]);
            Assert.Empty(claims[BigSmileClaimTypes.Role]);
            Assert.Empty(claims[BigSmileClaimTypes.BranchId]);
            Assert.Equal(IssuedAtUtc.AddHours(1), generated.ExpiresAtUtc);
        }

        [Fact]
        public void GenerateForIntake_RejectsLinkedAccountOrForeignDraft()
        {
            var tenantId = Guid.NewGuid();
            var patient = new Patient(
                tenantId,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
            var linked = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "linked.patient",
                "versioned-password-hash",
                IssuedAtUtc);
            var unlinkedA = PatientPortalAccount.CreateUnlinked(
                tenantId,
                "waiting.a",
                "versioned-password-hash",
                IssuedAtUtc);
            var unlinkedB = PatientPortalAccount.CreateUnlinked(
                tenantId,
                "waiting.b",
                "versioned-password-hash",
                IssuedAtUtc);
            var intakeA = PatientIntake.CreateForNewPatient(
                unlinkedA,
                branch: null,
                IssuedAtUtc);
            var service = new PatientPortalJwtTokenService(new FixedJwtSettings());

            Assert.Throws<InvalidOperationException>(() =>
                service.GenerateForIntake(linked, intakeA, IssuedAtUtc));
            Assert.Throws<InvalidOperationException>(() =>
                service.GenerateForIntake(unlinkedB, intakeA, IssuedAtUtc));
        }

        private sealed class FixedJwtSettings : IPatientPortalJwtSettings
        {
            public string Secret =>
                "patient-intake-tests-secret-that-is-long-and-distinct";
            public string Issuer => "BigSmile.PatientIntake.Tests";
            public string Audience => "BigSmile.PatientIntake.Tests";
            public TimeSpan AccessTokenLifetime => TimeSpan.FromHours(1);
        }
    }
}
