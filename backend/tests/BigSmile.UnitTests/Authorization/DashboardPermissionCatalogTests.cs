using BigSmile.Application.Authorization;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class DashboardPermissionCatalogTests
    {
        private readonly RolePermissionCatalog _catalog = new();

        [Fact]
        public void DashboardRead_IsGrantedToTenantAdmin()
        {
            Assert.Contains(Permissions.DashboardRead, _catalog.GetPermissions(SystemRoles.TenantAdmin));
        }

        [Fact]
        public void DashboardRead_IsNotGrantedToTenantUser()
        {
            Assert.DoesNotContain(Permissions.DashboardRead, _catalog.GetPermissions(SystemRoles.TenantUser));
        }

        [Fact]
        public void DashboardRead_IsNotGrantedToPlatformAdminInCurrentSlice()
        {
            Assert.DoesNotContain(Permissions.DashboardRead, _catalog.GetPermissions(SystemRoles.PlatformAdmin));
        }
    }
}
