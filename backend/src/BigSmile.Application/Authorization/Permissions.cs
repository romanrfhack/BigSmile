namespace BigSmile.Application.Authorization
{
    public static class Permissions
    {
        public const string AuthSelfRead = "auth.self.read";
        public const string PlatformTenantsRead = "platform.tenants.read";
        public const string TenantRead = "tenant.read";
        public const string BranchReadAny = "branch.read.any";
        public const string BranchReadAssigned = "branch.read.assigned";
        public const string PatientRead = "patient.read";
        public const string PatientWrite = "patient.write";
    }
}
