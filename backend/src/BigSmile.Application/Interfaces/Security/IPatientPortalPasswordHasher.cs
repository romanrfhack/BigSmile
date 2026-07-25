namespace BigSmile.Application.Interfaces.Security
{
    public enum PatientPortalPasswordVerificationStatus
    {
        Failed = 0,
        Success = 1,
        SuccessRehashNeeded = 2
    }

    public interface IPatientPortalPasswordHasher
    {
        string HashPassword(string password);

        PatientPortalPasswordVerificationStatus VerifyHashedPassword(
            string passwordHash,
            string providedPassword);

        void PerformDummyVerification(string providedPassword);
    }
}
