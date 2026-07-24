using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalInvitationTests
    {
        private static readonly DateTime CreatedAtUtc = new(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Constructor_DerivesTenantAndPatientFromCanonicalPatient()
        {
            var patient = CreatePatient(Guid.NewGuid());
            var actorId = Guid.NewGuid();

            var invitation = new PatientPortalInvitation(
                patient,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                CreateTokenHash(),
                CreatedAtUtc,
                CreatedAtUtc.AddHours(24),
                actorId);

            Assert.Equal(patient.TenantId, invitation.TenantId);
            Assert.Equal(patient.Id, invitation.PatientId);
            Assert.Equal(PatientPortalInvitationPurpose.ExistingPatientActivation, invitation.Purpose);
            Assert.Equal(actorId, invitation.CreatedByUserId);
            Assert.False(invitation.CanBeConsumedAt(CreatedAtUtc.AddTicks(-1)));
            Assert.True(invitation.CanBeConsumedAt(CreatedAtUtc.AddHours(1)));
        }

        [Fact]
        public void Constructor_RejectsExpiryAtOrBeforeCreation()
        {
            Assert.Throws<ArgumentException>(() => new PatientPortalInvitation(
                CreatePatient(Guid.NewGuid()),
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                CreateTokenHash(),
                CreatedAtUtc,
                CreatedAtUtc,
                Guid.NewGuid()));
        }

        [Fact]
        public void Consume_RecordsMatchingAccountAndRejectsReplay()
        {
            var patient = CreatePatient(Guid.NewGuid());
            var invitation = CreateInvitation(patient);
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "patient-login",
                "versioned-password-hash");
            var consumedAtUtc = CreatedAtUtc.AddHours(1);

            invitation.Consume(account, consumedAtUtc);

            Assert.Equal(account.Id, invitation.ConsumedByPatientPortalAccountId);
            Assert.Equal(consumedAtUtc, invitation.ConsumedAtUtc);
            Assert.False(invitation.CanBeConsumedAt(consumedAtUtc.AddMinutes(1)));
            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(account, consumedAtUtc.AddMinutes(1)));
        }

        [Fact]
        public void Consume_RejectsAccountFromAnotherTenantOrPatient()
        {
            var patient = CreatePatient(Guid.NewGuid());
            var invitation = CreateInvitation(patient);
            var foreignAccount = PatientPortalAccount.CreateForExistingPatient(
                CreatePatient(Guid.NewGuid()),
                "foreign-login",
                "versioned-password-hash");

            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(foreignAccount, CreatedAtUtc.AddMinutes(10)));
        }

        [Fact]
        public void Consume_RejectsExpiredInvitation()
        {
            var patient = CreatePatient(Guid.NewGuid());
            var invitation = CreateInvitation(patient);
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "patient-login",
                "versioned-password-hash");

            Assert.True(invitation.IsExpiredAt(CreatedAtUtc.AddHours(24)));
            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(account, CreatedAtUtc.AddHours(24)));
        }

        [Fact]
        public void Revoke_RecordsActorAndBlocksConsumption()
        {
            var patient = CreatePatient(Guid.NewGuid());
            var invitation = CreateInvitation(patient);
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "patient-login",
                "versioned-password-hash");
            var actorId = Guid.NewGuid();
            var revokedAtUtc = CreatedAtUtc.AddMinutes(30);

            invitation.Revoke(actorId, revokedAtUtc);

            Assert.Equal(actorId, invitation.RevokedByUserId);
            Assert.Equal(revokedAtUtc, invitation.RevokedAtUtc);
            Assert.False(invitation.CanBeConsumedAt(revokedAtUtc.AddMinutes(1)));
            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(account, revokedAtUtc.AddMinutes(1)));
        }

        [Fact]
        public void Revoke_RejectsConsumedInvitation()
        {
            var patient = CreatePatient(Guid.NewGuid());
            var invitation = CreateInvitation(patient);
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "patient-login",
                "versioned-password-hash");
            invitation.Consume(account, CreatedAtUtc.AddMinutes(10));

            Assert.Throws<InvalidOperationException>(() =>
                invitation.Revoke(Guid.NewGuid(), CreatedAtUtc.AddMinutes(20)));
        }

        private static PatientPortalInvitation CreateInvitation(Patient patient)
        {
            return new PatientPortalInvitation(
                patient,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                CreateTokenHash(),
                CreatedAtUtc,
                CreatedAtUtc.AddHours(24),
                Guid.NewGuid());
        }

        private static Patient CreatePatient(Guid tenantId)
        {
            return new Patient(
                tenantId,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
        }

        private static string CreateTokenHash()
        {
            return new string('a', 64);
        }
    }
}
