using BigSmile.Api.Authorization;
using BigSmile.Application.Authorization;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class PatientIntakeRequestPermissionCatalogTests
    {
        private readonly RolePermissionCatalog _catalog = new();

        [Fact]
        public void RequestPermission_IsIndependentAndAvailableToReceptionRoles()
        {
            Assert.Contains(
                Permissions.PatientPortalIntakeRequest,
                _catalog.GetPermissions(SystemRoles.TenantAdmin));
            Assert.Contains(
                Permissions.PatientPortalIntakeRequest,
                _catalog.GetPermissions(SystemRoles.TenantUser));
            Assert.DoesNotContain(
                Permissions.PatientPortalIntakeRequest,
                _catalog.GetPermissions(SystemRoles.PlatformAdmin));

            Assert.DoesNotContain(
                Permissions.PatientPortalIntakeManage,
                _catalog.GetPermissions(SystemRoles.TenantUser));
            Assert.DoesNotContain(
                Permissions.PatientPortalInvitationManage,
                _catalog.GetPermissions(SystemRoles.TenantUser));
        }

        [Fact]
        public void RequestPolicy_RequiresResolvedTenantWithoutPlatformOverride()
        {
            var options = new Microsoft.AspNetCore.Authorization.AuthorizationOptions();
            AuthorizationPolicies.AddPolicies(options);

            var policy = options.GetPolicy(AuthorizationPolicies.PatientPortalIntakeRequest);
            Assert.NotNull(policy);

            var requirement = Assert.Single(
                policy!.Requirements.OfType<PermissionRequirement>());
            Assert.Equal(Permissions.PatientPortalIntakeRequest, requirement.Permission);
            Assert.True(requirement.RequireResolvedTenantContext);
            Assert.False(requirement.EnablePlatformOverride);
            Assert.False(requirement.RequirePlatformScope);
        }
    }
}
