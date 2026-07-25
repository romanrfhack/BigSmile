using BigSmile.Application.Interfaces.Security;
using Microsoft.AspNetCore.Identity;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalPasswordHasher : IPatientPortalPasswordHasher
    {
        private static readonly object HashUser = new();
        private readonly PasswordHasher<object> _hasher;
        private readonly string _dummyHash;

        public PatientPortalPasswordHasher(IPatientPortalAuthenticationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            _hasher = new PasswordHasher<object>(Microsoft.Extensions.Options.Options.Create(
                new PasswordHasherOptions
                {
                    CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3,
                    IterationCount = settings.PasswordHashIterationCount
                }));
            _dummyHash = _hasher.HashPassword(HashUser, "BigSmile.PatientPortal.Dummy.Password.Value");
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Patient portal password is required.", nameof(password));
            }

            return _hasher.HashPassword(HashUser, password);
        }

        public PatientPortalPasswordVerificationStatus VerifyHashedPassword(
            string passwordHash,
            string providedPassword)
        {
            if (string.IsNullOrWhiteSpace(passwordHash) || providedPassword is null)
            {
                return PatientPortalPasswordVerificationStatus.Failed;
            }

            try
            {
                return _hasher.VerifyHashedPassword(HashUser, passwordHash, providedPassword) switch
                {
                    PasswordVerificationResult.Success => PatientPortalPasswordVerificationStatus.Success,
                    PasswordVerificationResult.SuccessRehashNeeded => PatientPortalPasswordVerificationStatus.SuccessRehashNeeded,
                    _ => PatientPortalPasswordVerificationStatus.Failed
                };
            }
            catch (FormatException)
            {
                return PatientPortalPasswordVerificationStatus.Failed;
            }
            catch (ArgumentException)
            {
                return PatientPortalPasswordVerificationStatus.Failed;
            }
        }

        public void PerformDummyVerification(string providedPassword)
        {
            _ = _hasher.VerifyHashedPassword(HashUser, _dummyHash, providedPassword ?? string.Empty);
        }
    }
}
