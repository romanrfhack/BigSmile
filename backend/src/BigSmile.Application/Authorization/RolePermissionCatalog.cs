namespace BigSmile.Application.Authorization
{
    public interface IRolePermissionCatalog
    {
        IReadOnlyCollection<string> GetPermissions(string roleName);
    }

    public sealed class RolePermissionCatalog : IRolePermissionCatalog
    {
        private static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> PermissionsByRole =
            new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase)
            {
                [SystemRoles.PlatformAdmin] = new[]
                {
                    Permissions.AuthSelfRead,
                    Permissions.PlatformTenantsRead,
                    Permissions.TenantRead,
                    Permissions.BranchReadAny,
                    Permissions.BranchReadAssigned,
                    Permissions.PatientRead,
                    Permissions.PatientWrite,
                    Permissions.SchedulingRead,
                    Permissions.SchedulingWrite,
                    Permissions.ClinicalRead,
                    Permissions.ClinicalWrite,
                    Permissions.OdontogramRead,
                    Permissions.OdontogramWrite,
                    Permissions.TreatmentPlanRead,
                    Permissions.TreatmentPlanWrite,
                    Permissions.TreatmentQuoteRead,
                    Permissions.TreatmentQuoteWrite
                },
                [SystemRoles.TenantAdmin] = new[]
                {
                    Permissions.AuthSelfRead,
                    Permissions.TenantRead,
                    Permissions.BranchReadAny,
                    Permissions.PatientRead,
                    Permissions.PatientWrite,
                    Permissions.SchedulingRead,
                    Permissions.SchedulingWrite,
                    Permissions.ClinicalRead,
                    Permissions.ClinicalWrite,
                    Permissions.OdontogramRead,
                    Permissions.OdontogramWrite,
                    Permissions.TreatmentPlanRead,
                    Permissions.TreatmentPlanWrite,
                    Permissions.TreatmentQuoteRead,
                    Permissions.TreatmentQuoteWrite
                },
                [SystemRoles.TenantUser] = new[]
                {
                    Permissions.AuthSelfRead,
                    Permissions.TenantRead,
                    Permissions.BranchReadAssigned,
                    Permissions.PatientRead,
                    Permissions.PatientWrite,
                    Permissions.SchedulingRead,
                    Permissions.SchedulingWrite
                }
            };

        public IReadOnlyCollection<string> GetPermissions(string roleName)
        {
            return PermissionsByRole.TryGetValue(roleName, out var permissions)
                ? permissions
                : Array.Empty<string>();
        }
    }
}
