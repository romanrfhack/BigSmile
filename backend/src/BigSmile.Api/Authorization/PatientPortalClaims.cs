using BigSmile.Application.Interfaces.Security;
using BigSmile.SharedKernel.Authorization;
using System.Security.Claims;

namespace BigSmile.Api.Authorization
{
    public static class PatientPortalClaims
    {
        public static bool TryGetSessionIdentity(
            ClaimsPrincipal principal,
            out PatientPortalSessionIdentity identity)
        {
            ArgumentNullException.ThrowIfNull(principal);

            identity = default!;
            if (!HasBoundedIdentityShape(
                    principal,
                    AccessScope.Patient,
                    forbiddenOwnershipClaim: BigSmileClaimTypes.IntakeId))
            {
                return false;
            }

            var accountValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantValue = principal.FindFirst(BigSmileClaimTypes.TenantId)?.Value;
            var patientValue = principal.FindFirst(BigSmileClaimTypes.PatientId)?.Value;
            var sessionVersionValue = principal.FindFirst(BigSmileClaimTypes.SessionVersion)?.Value;

            if (!Guid.TryParse(accountValue, out var accountId) || accountId == Guid.Empty ||
                !Guid.TryParse(tenantValue, out var tenantId) || tenantId == Guid.Empty ||
                !Guid.TryParse(patientValue, out var patientId) || patientId == Guid.Empty ||
                !int.TryParse(sessionVersionValue, out var sessionVersion) || sessionVersion <= 0)
            {
                return false;
            }

            identity = new PatientPortalSessionIdentity(
                accountId,
                tenantId,
                patientId,
                sessionVersion);
            return true;
        }

        public static bool TryGetIntakeSessionIdentity(
            ClaimsPrincipal principal,
            out PatientIntakeSessionIdentity identity)
        {
            ArgumentNullException.ThrowIfNull(principal);

            identity = default!;
            if (!HasBoundedIdentityShape(
                    principal,
                    AccessScope.PatientIntake,
                    forbiddenOwnershipClaim: BigSmileClaimTypes.PatientId))
            {
                return false;
            }

            var accountValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var tenantValue = principal.FindFirst(BigSmileClaimTypes.TenantId)?.Value;
            var intakeValue = principal.FindFirst(BigSmileClaimTypes.IntakeId)?.Value;
            var sessionVersionValue = principal.FindFirst(BigSmileClaimTypes.SessionVersion)?.Value;

            if (!Guid.TryParse(accountValue, out var accountId) || accountId == Guid.Empty ||
                !Guid.TryParse(tenantValue, out var tenantId) || tenantId == Guid.Empty ||
                !Guid.TryParse(intakeValue, out var intakeId) || intakeId == Guid.Empty ||
                !int.TryParse(sessionVersionValue, out var sessionVersion) || sessionVersion <= 0)
            {
                return false;
            }

            identity = new PatientIntakeSessionIdentity(
                accountId,
                tenantId,
                intakeId,
                sessionVersion);
            return true;
        }

        private static bool HasBoundedIdentityShape(
            ClaimsPrincipal principal,
            AccessScope requiredScope,
            string forbiddenOwnershipClaim)
        {
            return principal.Identity?.IsAuthenticated == true &&
                   string.Equals(
                       principal.FindFirst(BigSmileClaimTypes.Scope)?.Value,
                       requiredScope.ToClaimValue(),
                       StringComparison.OrdinalIgnoreCase) &&
                   !principal.HasClaim(claim => claim.Type == forbiddenOwnershipClaim) &&
                   !principal.Claims.Any(claim => claim.Type == BigSmileClaimTypes.Permission) &&
                   !principal.Claims.Any(claim =>
                       claim.Type is ClaimTypes.Role or BigSmileClaimTypes.Role) &&
                   !principal.HasClaim(claim =>
                       claim.Type is BigSmileClaimTypes.BranchId or BigSmileClaimTypes.BranchName);
        }
    }
}
