using BigSmile.Api.Authorization;
using BigSmile.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class PatientPortalInvitationPermissionCatalogTests
    {
        private readonly RolePermissionCatalog _catalog = new();

        [Fact]
        public void InvitationManage_IsGrantedOnlyToTenantAdmin()
        {
            Assert.Contains(
                Permissions.PatientPortalInvitationManage,
                _catalog.GetPermissions(SystemRoles.TenantAdmin));
            Assert.DoesNotContain(
                Permissions.PatientPortalInvitationManage,
                _catalog.GetPermissions(SystemRoles.TenantUser));
            Assert.DoesNotContain(
                Permissions.PatientPortalInvitationManage,
                _catalog.GetPermissions(SystemRoles.PlatformAdmin));
        }

        [Fact]
        public void InvitationManagePolicy_RequiresTenantContextAndDoesNotEnablePlatformOverride()
        {
            var options = new AuthorizationOptions();
            AuthorizationPolicies.AddPolicies(options);

            var policy = options.GetPolicy(AuthorizationPolicies.PatientPortalInvitationManage);
            Assert.NotNull(policy);

            var requirement = Assert.Single(policy!.Requirements.OfType<PermissionRequirement>());
            Assert.Equal(Permissions.PatientPortalInvitationManage, requirement.Permission);
            Assert.True(requirement.RequireResolvedTenantContext);
            Assert.False(requirement.EnablePlatformOverride);
            Assert.False(requirement.RequirePlatformScope);
        }
    }
}
