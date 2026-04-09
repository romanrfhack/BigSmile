namespace BigSmile.SharedKernel.Authorization
{
    public static class AccessScopeExtensions
    {
        public static string ToClaimValue(this AccessScope scope)
        {
            return scope switch
            {
                AccessScope.Tenant => "tenant",
                AccessScope.Branch => "branch",
                AccessScope.Platform => "platform",
                _ => "anonymous"
            };
        }

        public static AccessScope ToAccessScope(this string? value)
        {
            return value?.Trim().ToLowerInvariant() switch
            {
                "tenant" => AccessScope.Tenant,
                "branch" => AccessScope.Branch,
                "platform" => AccessScope.Platform,
                _ => AccessScope.Anonymous
            };
        }
    }
}
