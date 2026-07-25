namespace BigSmile.Api.Authorization
{
    public static class PatientPortalAuthenticationDefaults
    {
        public const string SelectorScheme = "BigSmileBearerSelector";
        public const string PatientBearerScheme = "PatientPortalBearer";
        public const string PatientSelfPolicy = "patientportal.self";
        public const string IntakeOnlyPolicy = "patientportal.intake-only";
        public const string PatientIntakeSelfPolicy = "patientportal.intake.self";
        public const string PatientPathPrefix = "/api/patient-portal";
    }
}
