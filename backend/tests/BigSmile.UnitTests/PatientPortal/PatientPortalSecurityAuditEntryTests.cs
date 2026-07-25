using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalSecurityAuditEntryTests
    {
        private static readonly DateTime OccurredAtUtc = new(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Constructor_DerivesTenantPatientAndInvitationOwnership()
        {
            var invitation = CreateInvitation();
            var actorId = Guid.NewGuid();

            var entry = new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationIssued,
                actorId,
                OccurredAtUtc,
                " correlation-123 ");

            Assert.Equal(invitation.TenantId, entry.TenantId);
            Assert.Equal(invitation.PatientId, entry.PatientId);
            Assert.Equal(invitation.Id, entry.PatientPortalInvitationId);
            Assert.Equal(actorId, entry.ActorUserId);
            Assert.Equal("correlation-123", entry.CorrelationId);
        }

        [Fact]
        public void Constructor_RejectsInvalidActorTimestampOrCorrelation()
        {
            var invitation = CreateInvitation();

            Assert.Throws<ArgumentException>(() => new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationIssued,
                Guid.Empty,
                OccurredAtUtc,
                "correlation"));
            Assert.Throws<ArgumentException>(() => new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationIssued,
                Guid.NewGuid(),
                DateTime.SpecifyKind(OccurredAtUtc, DateTimeKind.Local),
                "correlation"));
            Assert.Throws<ArgumentException>(() => new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationIssued,
                Guid.NewGuid(),
                OccurredAtUtc,
                " "));
        }

        private static PatientPortalInvitation CreateInvitation()
        {
            var patient = new Patient(
                Guid.NewGuid(),
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
            return new PatientPortalInvitation(
                patient,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                new string('a', 64),
                OccurredAtUtc,
                OccurredAtUtc.AddHours(24),
                Guid.NewGuid());
        }
    }
}
