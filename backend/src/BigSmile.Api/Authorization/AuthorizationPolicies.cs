using BigSmile.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace BigSmile.Api.Authorization
{
    public static class AuthorizationPolicies
    {
        public const string SelfRead = "auth.self.read";
        public const string PlatformTenantsRead = "platform.tenants.read";
        public const string TenantRead = "tenant.read";
        public const string BranchRead = "branch.read";

        public static void AddPolicies(AuthorizationOptions options)
        {
            options.AddPolicy(SelfRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.AuthSelfRead));
            });

            options.AddPolicy(PlatformTenantsRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(
                    Permissions.PlatformTenantsRead,
                    requirePlatformScope: true,
                    enablePlatformOverride: true));
            });

            options.AddPolicy(TenantRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new TenantAccessRequirement(
                    Permissions.TenantRead,
                    routeValueKey: "id",
                    allowPlatformOverride: true));
            });

            options.AddPolicy(BranchRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new BranchAccessRequirement(
                    routeValueKey: "id",
                    allowPlatformOverride: true));
            });
        }
    }
}
