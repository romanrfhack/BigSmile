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
                AccessScope.Patient => "patient",
                AccessScope.PatientIntake => "patient_intake",
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
                "patient" => AccessScope.Patient,
                "patient_intake" => AccessScope.PatientIntake,
                _ => AccessScope.Anonymous
            };
        }
    }
}
