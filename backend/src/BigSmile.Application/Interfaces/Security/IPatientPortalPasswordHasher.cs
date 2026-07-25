using BigSmile.Domain.Entities;

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
        string HashPassword(PatientPortalAccount account, string password);

        PatientPortalPasswordVerificationStatus VerifyHashedPassword(
            PatientPortalAccount account,
            string passwordHash,
            string providedPassword);

        void PerformDummyVerification(string providedPassword);
    }
}
