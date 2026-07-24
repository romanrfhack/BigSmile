using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalAccountTests
    {
        [Fact]
        public void CreateForExistingPatient_NormalizesLoginAndPreservesOwnership()
        {
            var patient = CreatePatient(Guid.NewGuid());

            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "  Patient.Name@example.com  ",
                "versioned-password-hash");

            Assert.Equal(patient.TenantId, account.TenantId);
            Assert.Equal(patient.Id, account.PatientId);
            Assert.Equal("Patient.Name@example.com", account.LoginName);
            Assert.Equal("PATIENT.NAME@EXAMPLE.COM", account.NormalizedLoginName);
            Assert.True(account.IsActive);
            Assert.Equal(1, account.SessionVersion);
            Assert.Equal(0, account.FailedLoginAttempts);
        }

        [Theory]
        [InlineData("")]
        [InlineData("ab")]
        [InlineData("patient name")]
        public void CreateUnlinked_RejectsInvalidLoginName(string loginName)
        {
            Assert.Throws<ArgumentException>(() => PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                loginName,
                "versioned-password-hash"));
        }

        [Fact]
        public void LinkPatient_AllowsOneCanonicalPatientInTheSameTenantOnly()
        {
            var tenantId = Guid.NewGuid();
            var account = PatientPortalAccount.CreateUnlinked(
                tenantId,
                "patient-login",
                "versioned-password-hash");
            var patient = CreatePatient(tenantId);

            account.LinkPatient(patient);
            account.LinkPatient(patient);

            Assert.Equal(patient.Id, account.PatientId);
            Assert.Throws<InvalidOperationException>(() => account.LinkPatient(CreatePatient(tenantId)));
            Assert.Throws<InvalidOperationException>(() => account.LinkPatient(CreatePatient(Guid.NewGuid())));
        }

        [Fact]
        public void RegisterFailedLogin_LocksAtConfiguredThreshold()
        {
            var account = CreateAccount();
            var occurredAtUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

            for (var attempt = 1; attempt < 5; attempt++)
            {
                Assert.False(account.RegisterFailedLogin(
                    occurredAtUtc.AddSeconds(attempt),
                    maxFailedAttempts: 5,
                    lockoutDuration: TimeSpan.FromMinutes(15)));
            }

            var locked = account.RegisterFailedLogin(
                occurredAtUtc.AddSeconds(5),
                maxFailedAttempts: 5,
                lockoutDuration: TimeSpan.FromMinutes(15));

            Assert.True(locked);
            Assert.Equal(5, account.FailedLoginAttempts);
            Assert.Equal(occurredAtUtc.AddSeconds(5).AddMinutes(15), account.LockoutEndUtc);
            Assert.True(account.IsLockedOutAt(occurredAtUtc.AddMinutes(1)));
        }

        [Fact]
        public void RegisterFailedLogin_AfterExpiredLockoutStartsANewAttemptWindow()
        {
            var account = CreateAccount();
            var occurredAtUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

            for (var attempt = 1; attempt <= 5; attempt++)
            {
                account.RegisterFailedLogin(
                    occurredAtUtc.AddSeconds(attempt),
                    maxFailedAttempts: 5,
                    lockoutDuration: TimeSpan.FromMinutes(15));
            }

            var afterExpiryUtc = occurredAtUtc.AddMinutes(16);
            var locked = account.RegisterFailedLogin(
                afterExpiryUtc,
                maxFailedAttempts: 5,
                lockoutDuration: TimeSpan.FromMinutes(15));

            Assert.False(locked);
            Assert.Equal(1, account.FailedLoginAttempts);
            Assert.Null(account.LockoutEndUtc);
            Assert.Equal(afterExpiryUtc, account.LastFailedLoginAtUtc);
        }

        [Fact]
        public void RegisterSuccessfulLogin_ClearsFailedAttemptsAndLockout()
        {
            var account = CreateAccount();
            var occurredAtUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

            for (var attempt = 1; attempt <= 5; attempt++)
            {
                account.RegisterFailedLogin(
                    occurredAtUtc.AddSeconds(attempt),
                    maxFailedAttempts: 5,
                    lockoutDuration: TimeSpan.FromMinutes(15));
            }

            var successfulAtUtc = occurredAtUtc.AddMinutes(16);
            account.RegisterSuccessfulLogin(successfulAtUtc);

            Assert.Equal(0, account.FailedLoginAttempts);
            Assert.Null(account.LockoutEndUtc);
            Assert.Equal(successfulAtUtc, account.LastSuccessfulLoginAtUtc);
        }

        [Fact]
        public void RecoverAccess_ReplacesHashClearsLockoutAndRotatesSessionVersion()
        {
            var account = CreateAccount();
            var occurredAtUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

            for (var attempt = 1; attempt <= 5; attempt++)
            {
                account.RegisterFailedLogin(
                    occurredAtUtc.AddSeconds(attempt),
                    maxFailedAttempts: 5,
                    lockoutDuration: TimeSpan.FromMinutes(15));
            }

            account.RecoverAccess("replacement-versioned-hash", occurredAtUtc.AddMinutes(2));

            Assert.Equal("replacement-versioned-hash", account.PasswordHash);
            Assert.Equal(0, account.FailedLoginAttempts);
            Assert.Null(account.LockoutEndUtc);
            Assert.Null(account.LastFailedLoginAtUtc);
            Assert.True(account.IsActive);
            Assert.Equal(2, account.SessionVersion);
        }

        private static PatientPortalAccount CreateAccount()
        {
            return PatientPortalAccount.CreateForExistingPatient(
                CreatePatient(Guid.NewGuid()),
                "patient-login",
                "versioned-password-hash");
        }

        private static Patient CreatePatient(Guid tenantId)
        {
            return new Patient(
                tenantId,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
        }
    }
}
