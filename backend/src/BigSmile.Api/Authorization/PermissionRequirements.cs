using Microsoft.AspNetCore.Authorization;

namespace BigSmile.Api.Authorization
{
    public sealed class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission, bool requirePlatformScope = false, bool enablePlatformOverride = false)
        {
            Permission = permission;
            RequirePlatformScope = requirePlatformScope;
            EnablePlatformOverride = enablePlatformOverride;
        }

        public string Permission { get; }
        public bool RequirePlatformScope { get; }
        public bool EnablePlatformOverride { get; }
    }

    public sealed class TenantAccessRequirement : IAuthorizationRequirement
    {
        public TenantAccessRequirement(string permission, string routeValueKey, bool allowPlatformOverride)
        {
            Permission = permission;
            RouteValueKey = routeValueKey;
            AllowPlatformOverride = allowPlatformOverride;
        }

        public string Permission { get; }
        public string RouteValueKey { get; }
        public bool AllowPlatformOverride { get; }
    }

    public sealed class BranchAccessRequirement : IAuthorizationRequirement
    {
        public BranchAccessRequirement(string routeValueKey, bool allowPlatformOverride)
        {
            RouteValueKey = routeValueKey;
            AllowPlatformOverride = allowPlatformOverride;
        }

        public string RouteValueKey { get; }
        public bool AllowPlatformOverride { get; }
    }
}
