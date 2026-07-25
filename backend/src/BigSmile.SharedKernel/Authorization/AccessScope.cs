namespace BigSmile.SharedKernel.Authorization
{
    public enum AccessScope
    {
        Anonymous = 0,
        Tenant = 1,
        Branch = 2,
        Platform = 3,
        Patient = 4,
        PatientIntake = 5
    }
}
