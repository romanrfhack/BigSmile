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
        public const string PatientRead = "patient.read";
        public const string PatientWrite = "patient.write";
        public const string SchedulingRead = "scheduling.read";
        public const string SchedulingWrite = "scheduling.write";
        public const string ClinicalRead = "clinical.read";
        public const string ClinicalWrite = "clinical.write";
        public const string OdontogramRead = "odontogram.read";
        public const string OdontogramWrite = "odontogram.write";
        public const string TreatmentPlanRead = "treatmentplan.read";
        public const string TreatmentPlanWrite = "treatmentplan.write";
        public const string TreatmentQuoteRead = "treatmentquote.read";
        public const string TreatmentQuoteWrite = "treatmentquote.write";
        public const string BillingRead = "billing.read";
        public const string BillingWrite = "billing.write";
        public const string DocumentRead = "document.read";
        public const string DocumentWrite = "document.write";
        public const string DashboardRead = "dashboard.read";

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

            options.AddPolicy(PatientRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.PatientRead));
            });

            options.AddPolicy(PatientWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.PatientWrite));
            });

            options.AddPolicy(SchedulingRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.SchedulingRead));
            });

            options.AddPolicy(SchedulingWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.SchedulingWrite));
            });

            options.AddPolicy(ClinicalRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.ClinicalRead));
            });

            options.AddPolicy(ClinicalWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.ClinicalWrite));
            });

            options.AddPolicy(OdontogramRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.OdontogramRead));
            });

            options.AddPolicy(OdontogramWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.OdontogramWrite));
            });

            options.AddPolicy(TreatmentPlanRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.TreatmentPlanRead));
            });

            options.AddPolicy(TreatmentPlanWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.TreatmentPlanWrite));
            });

            options.AddPolicy(TreatmentQuoteRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.TreatmentQuoteRead));
            });

            options.AddPolicy(TreatmentQuoteWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.TreatmentQuoteWrite));
            });

            options.AddPolicy(BillingRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.BillingRead));
            });

            options.AddPolicy(BillingWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(Permissions.BillingWrite));
            });

            options.AddPolicy(DocumentRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(
                    Permissions.DocumentRead,
                    enablePlatformOverride: true,
                    platformOverrideRouteValueKey: "patientId"));
            });

            options.AddPolicy(DocumentWrite, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(
                    Permissions.DocumentWrite,
                    enablePlatformOverride: true,
                    platformOverrideRouteValueKey: "patientId"));
            });

            options.AddPolicy(DashboardRead, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(
                    Permissions.DashboardRead,
                    requireResolvedTenantContext: true));
            });
        }
    }
}
