using BigSmile.Application.Authorization;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class PatientPortalRecoveryPermissionCatalogTests
    {
        private readonly RolePermissionCatalog _catalog = new();

        [Fact]
        public void RecoveryPermission_IsGrantedOnlyToTenantAdmin()
        {
            Assert.Contains(
                Permissions.PatientPortalAccountRecover,
                _catalog.GetPermissions(SystemRoles.TenantAdmin));
            Assert.DoesNotContain(
                Permissions.PatientPortalAccountRecover,
                _catalog.GetPermissions(SystemRoles.TenantUser));
            Assert.DoesNotContain(
                Permissions.PatientPortalAccountRecover,
                _catalog.GetPermissions(SystemRoles.PlatformAdmin));
        }
    }
}
