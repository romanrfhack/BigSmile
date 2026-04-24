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
    }
}
