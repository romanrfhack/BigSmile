using BigSmile.Application.Interfaces.Security;
using BigSmile.Infrastructure.Services;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalPasswordHasherTests
    {
        [Fact]
        public void HashAndVerify_UsesVersionedIdentityFormat()
        {
            var hasher = new PatientPortalPasswordHasher(new FixedSettings(100_000));

            var hash = hasher.HashPassword("A sufficiently long patient password.");

            Assert.False(string.IsNullOrWhiteSpace(hash));
            Assert.NotEqual("A sufficiently long patient password.", hash);
            Assert.Equal(
                PatientPortalPasswordVerificationStatus.Success,
                hasher.VerifyHashedPassword(hash, "A sufficiently long patient password."));
            Assert.Equal(
                PatientPortalPasswordVerificationStatus.Failed,
                hasher.VerifyHashedPassword(hash, "An incorrect patient password."));
        }

        [Fact]
        public void Verify_ReturnsRehashNeeded_WhenConfiguredWorkFactorIncreases()
        {
            var originalHasher = new PatientPortalPasswordHasher(new FixedSettings(100_000));
            var upgradedHasher = new PatientPortalPasswordHasher(new FixedSettings(120_000));
            var hash = originalHasher.HashPassword("A sufficiently long patient password.");

            var result = upgradedHasher.VerifyHashedPassword(
                hash,
                "A sufficiently long patient password.");

            Assert.Equal(PatientPortalPasswordVerificationStatus.SuccessRehashNeeded, result);
        }

        [Fact]
        public void Verify_RejectsMalformedHash_AndDummyVerificationIsSafe()
        {
            var hasher = new PatientPortalPasswordHasher(new FixedSettings(100_000));

            Assert.Equal(
                PatientPortalPasswordVerificationStatus.Failed,
                hasher.VerifyHashedPassword("not-an-identity-hash", "patient-password"));
            hasher.PerformDummyVerification("patient-password");
        }

        private sealed class FixedSettings : IPatientPortalAuthenticationSettings
        {
            public FixedSettings(int iterationCount)
            {
                PasswordHashIterationCount = iterationCount;
            }

            public int PasswordHashIterationCount { get; }
            public int MinimumPasswordLength => 12;
            public int MaximumPasswordLength => 128;
            public int MaximumFailedLoginAttempts => 5;
            public TimeSpan LockoutDuration => TimeSpan.FromMinutes(15);
        }
    }
}
