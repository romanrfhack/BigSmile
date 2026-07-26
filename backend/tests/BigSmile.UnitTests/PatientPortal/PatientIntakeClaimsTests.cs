using System.Security.Claims;
using BigSmile.Api.Authorization;
using BigSmile.SharedKernel.Authorization;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeClaimsTests
    {
        [Fact]
        public void TryGetIntakeSessionIdentity_AcceptsOnlyBoundedIntakeShape()
        {
            var accountId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var intakeId = Guid.NewGuid();
            var principal = BuildPrincipal(
                accountId,
                tenantId,
                intakeId,
                sessionVersion: 3);

            var parsed = PatientPortalClaims.TryGetIntakeSessionIdentity(
                principal,
                out var identity);

            Assert.True(parsed);
            Assert.Equal(accountId, identity.AccountId);
            Assert.Equal(tenantId, identity.TenantId);
            Assert.Equal(intakeId, identity.IntakeId);
            Assert.Equal(3, identity.SessionVersion);
            Assert.False(PatientPortalClaims.TryGetSessionIdentity(
                principal,
                out _));
        }

        [Theory]
        [InlineData(BigSmileClaimTypes.PatientId)]
        [InlineData(BigSmileClaimTypes.Permission)]
        [InlineData(BigSmileClaimTypes.Role)]
        [InlineData(BigSmileClaimTypes.BranchId)]
        public void TryGetIntakeSessionIdentity_RejectsForbiddenClaims(
            string forbiddenClaimType)
        {
            var principal = BuildPrincipal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                sessionVersion: 1,
                new Claim(forbiddenClaimType, Guid.NewGuid().ToString()));

            Assert.False(PatientPortalClaims.TryGetIntakeSessionIdentity(
                principal,
                out _));
        }

        [Fact]
        public void TryGetIntakeSessionIdentity_RejectsPatientScopeOrMissingIntake()
        {
            var accountId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var intakeId = Guid.NewGuid();
            var patientScope = BuildPrincipal(
                accountId,
                tenantId,
                intakeId,
                sessionVersion: 1,
                scope: AccessScope.Patient.ToClaimValue());
            var missingIntake = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, accountId.ToString()),
                new Claim(BigSmileClaimTypes.TenantId, tenantId.ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.PatientIntake.ToClaimValue()),
                new Claim(BigSmileClaimTypes.SessionVersion, "1")
            }, "PatientPortalBearer"));

            Assert.False(PatientPortalClaims.TryGetIntakeSessionIdentity(
                patientScope,
                out _));
            Assert.False(PatientPortalClaims.TryGetIntakeSessionIdentity(
                missingIntake,
                out _));
        }

        private static ClaimsPrincipal BuildPrincipal(
            Guid accountId,
            Guid tenantId,
            Guid intakeId,
            int sessionVersion,
            Claim? additionalClaim = null,
            string? scope = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, accountId.ToString()),
                new(BigSmileClaimTypes.TenantId, tenantId.ToString()),
                new(BigSmileClaimTypes.IntakeId, intakeId.ToString()),
                new(BigSmileClaimTypes.Scope, scope ?? AccessScope.PatientIntake.ToClaimValue()),
                new(BigSmileClaimTypes.SessionVersion, sessionVersion.ToString())
            };
            if (additionalClaim is not null)
            {
                claims.Add(additionalClaim);
            }

            return new ClaimsPrincipal(new ClaimsIdentity(
                claims,
                "PatientPortalBearer"));
        }
    }
}
