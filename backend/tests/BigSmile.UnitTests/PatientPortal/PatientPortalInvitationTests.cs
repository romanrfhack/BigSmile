using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalInvitationTests
    {
        private static readonly DateTime CreatedAtUtc = new(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Constructor_PreservesTenantPatientPurposeAndHashedToken()
        {
            var tenantId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var actorId = Guid.NewGuid();

            var invitation = new PatientPortalInvitation(
                tenantId,
                patientId,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                CreateTokenHash(),
                CreatedAtUtc,
                CreatedAtUtc.AddHours(24),
                actorId);

            Assert.Equal(tenantId, invitation.TenantId);
            Assert.Equal(patientId, invitation.PatientId);
            Assert.Equal(PatientPortalInvitationPurpose.ExistingPatientActivation, invitation.Purpose);
            Assert.Equal(actorId, invitation.CreatedByUserId);
            Assert.True(invitation.CanBeConsumedAt(CreatedAtUtc.AddHours(1)));
        }

        [Fact]
        public void Constructor_RejectsExpiryAtOrBeforeCreation()
        {
            Assert.Throws<ArgumentException>(() => new PatientPortalInvitation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                CreateTokenHash(),
                CreatedAtUtc,
                CreatedAtUtc,
                Guid.NewGuid()));
        }

        [Fact]
        public void Consume_RecordsAccountAndRejectsReplay()
        {
            var invitation = CreateInvitation();
            var accountId = Guid.NewGuid();
            var consumedAtUtc = CreatedAtUtc.AddHours(1);

            invitation.Consume(accountId, consumedAtUtc);

            Assert.Equal(accountId, invitation.ConsumedByPatientPortalAccountId);
            Assert.Equal(consumedAtUtc, invitation.ConsumedAtUtc);
            Assert.False(invitation.CanBeConsumedAt(consumedAtUtc.AddMinutes(1)));
            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(Guid.NewGuid(), consumedAtUtc.AddMinutes(1)));
        }

        [Fact]
        public void Consume_RejectsExpiredInvitation()
        {
            var invitation = CreateInvitation();

            Assert.True(invitation.IsExpiredAt(CreatedAtUtc.AddHours(24)));
            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(Guid.NewGuid(), CreatedAtUtc.AddHours(24)));
        }

        [Fact]
        public void Revoke_RecordsActorAndBlocksConsumption()
        {
            var invitation = CreateInvitation();
            var actorId = Guid.NewGuid();
            var revokedAtUtc = CreatedAtUtc.AddMinutes(30);

            invitation.Revoke(actorId, revokedAtUtc);

            Assert.Equal(actorId, invitation.RevokedByUserId);
            Assert.Equal(revokedAtUtc, invitation.RevokedAtUtc);
            Assert.False(invitation.CanBeConsumedAt(revokedAtUtc.AddMinutes(1)));
            Assert.Throws<InvalidOperationException>(() =>
                invitation.Consume(Guid.NewGuid(), revokedAtUtc.AddMinutes(1)));
        }

        [Fact]
        public void Revoke_RejectsConsumedInvitation()
        {
            var invitation = CreateInvitation();
            invitation.Consume(Guid.NewGuid(), CreatedAtUtc.AddMinutes(10));

            Assert.Throws<InvalidOperationException>(() =>
                invitation.Revoke(Guid.NewGuid(), CreatedAtUtc.AddMinutes(20)));
        }

        private static PatientPortalInvitation CreateInvitation()
        {
            return new PatientPortalInvitation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                CreateTokenHash(),
                CreatedAtUtc,
                CreatedAtUtc.AddHours(24),
                Guid.NewGuid());
        }

        private static string CreateTokenHash()
        {
            return new string('a', 64);
        }
    }
}
