using BigSmile.Api.Authorization;
using BigSmile.SharedKernel.Authorization;
using System.Security.Claims;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class PatientPortalClaimsTests
    {
        [Fact]
        public void TryGetSessionIdentity_AcceptsBoundedPatientClaims()
        {
            var accountId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var principal = BuildPrincipal(
                accountId,
                tenantId,
                patientId,
                sessionVersion: 3);

            var parsed = PatientPortalClaims.TryGetSessionIdentity(principal, out var identity);

            Assert.True(parsed);
            Assert.Equal(accountId, identity.AccountId);
            Assert.Equal(tenantId, identity.TenantId);
            Assert.Equal(patientId, identity.PatientId);
            Assert.Equal(3, identity.SessionVersion);
        }

        [Theory]
        [InlineData(BigSmileClaimTypes.Permission, "patient.read")]
        [InlineData(BigSmileClaimTypes.Role, "TenantAdmin")]
        [InlineData(ClaimTypes.Role, "TenantAdmin")]
        [InlineData(BigSmileClaimTypes.BranchId, "a93dadf3-209c-48b5-b860-53784395e799")]
        public void TryGetSessionIdentity_RejectsStaffOrBranchClaims(string claimType, string claimValue)
        {
            var principal = BuildPrincipal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                sessionVersion: 1,
                new Claim(claimType, claimValue));

            Assert.False(PatientPortalClaims.TryGetSessionIdentity(principal, out _));
        }

        private static ClaimsPrincipal BuildPrincipal(
            Guid accountId,
            Guid tenantId,
            Guid patientId,
            int sessionVersion,
            params Claim[] extraClaims)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, accountId.ToString()),
                new(BigSmileClaimTypes.TenantId, tenantId.ToString()),
                new(BigSmileClaimTypes.PatientId, patientId.ToString()),
                new(BigSmileClaimTypes.Scope, AccessScope.Patient.ToClaimValue()),
                new(BigSmileClaimTypes.SessionVersion, sessionVersion.ToString())
            };
            claims.AddRange(extraClaims);
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "PatientPortalBearer"));
        }
    }
}
