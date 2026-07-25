using BigSmile.Api.Authorization;
using BigSmile.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class PatientIntakeAccessLinkPermissionCatalogTests
    {
        private readonly RolePermissionCatalog _catalog = new();

        [Fact]
        public void IntakeManage_IsGrantedOnlyToTenantAdmin()
        {
            Assert.Contains(
                Permissions.PatientPortalIntakeManage,
                _catalog.GetPermissions(SystemRoles.TenantAdmin));
            Assert.DoesNotContain(
                Permissions.PatientPortalIntakeManage,
                _catalog.GetPermissions(SystemRoles.TenantUser));
            Assert.DoesNotContain(
                Permissions.PatientPortalIntakeManage,
                _catalog.GetPermissions(SystemRoles.PlatformAdmin));
        }

        [Fact]
        public void IntakeManagePolicy_RequiresTenantContextAndDoesNotEnablePlatformOverride()
        {
            var options = new AuthorizationOptions();
            AuthorizationPolicies.AddPolicies(options);

            var policy = options.GetPolicy(
                AuthorizationPolicies.PatientIntakeAccessLinkManage);
            Assert.NotNull(policy);

            var requirement = Assert.Single(
                policy!.Requirements.OfType<PermissionRequirement>());
            Assert.Equal(Permissions.PatientPortalIntakeManage, requirement.Permission);
            Assert.True(requirement.RequireResolvedTenantContext);
            Assert.False(requirement.EnablePlatformOverride);
            Assert.False(requirement.RequirePlatformScope);
        }
    }
}
