using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAccessLinkTests
    {
        private static readonly DateTime CreatedAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Constructor_PreservesTenantAndOptionalBranchOwnership()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");

            var link = CreateLink(tenant.Id, branch);

            Assert.Equal(tenant.Id, link.TenantId);
            Assert.Equal(branch.Id, link.BranchId);
            Assert.Equal(
                PatientIntakeAccessLinkPurpose.NewPatientWaitingRoomRegistration,
                link.Purpose);
            Assert.True(link.CanBeConsumedAt(CreatedAtUtc.AddMinutes(1)));
            Assert.False(link.IsExpiredAt(CreatedAtUtc.AddMinutes(29)));
            Assert.True(link.IsExpiredAt(CreatedAtUtc.AddMinutes(30)));
        }

        [Fact]
        public void Constructor_RejectsForeignOrInactiveBranch()
        {
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            var foreignBranch = tenantB.AddBranch("Foreign");
            var inactiveBranch = tenantA.AddBranch("Inactive");
            inactiveBranch.Deactivate();

            Assert.Throws<InvalidOperationException>(() =>
                CreateLink(tenantA.Id, foreignBranch));
            Assert.Throws<InvalidOperationException>(() =>
                CreateLink(tenantA.Id, inactiveBranch));
        }

        [Fact]
        public void Revoke_ClosesOnlyAnActiveLink()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var link = CreateLink(tenant.Id, branch: null);
            var actorId = Guid.NewGuid();

            link.Revoke(actorId, CreatedAtUtc.AddMinutes(5));

            Assert.Equal(CreatedAtUtc.AddMinutes(5), link.RevokedAtUtc);
            Assert.Equal(actorId, link.RevokedByUserId);
            Assert.False(link.CanBeConsumedAt(CreatedAtUtc.AddMinutes(6)));
            Assert.Throws<InvalidOperationException>(() =>
                link.Revoke(actorId, CreatedAtUtc.AddMinutes(7)));
        }

        [Fact]
        public void Consume_RequiresUnlinkedSameTenantAccountAndRejectsReplay()
        {
            var tenantId = Guid.NewGuid();
            var link = CreateLink(tenantId, branch: null);
            var account = PatientPortalAccount.CreateUnlinked(
                tenantId,
                "new.patient",
                "versioned-password-hash");

            link.Consume(account, CreatedAtUtc.AddMinutes(5));

            Assert.Equal(account.Id, link.ConsumedByPatientPortalAccountId);
            Assert.Equal(CreatedAtUtc.AddMinutes(5), link.ConsumedAtUtc);
            Assert.False(link.CanBeConsumedAt(CreatedAtUtc.AddMinutes(6)));
            Assert.Throws<InvalidOperationException>(() =>
                link.Consume(account, CreatedAtUtc.AddMinutes(7)));
        }

        [Fact]
        public void Consume_RejectsLinkedOrCrossTenantAccount()
        {
            var tenantId = Guid.NewGuid();
            var link = CreateLink(tenantId, branch: null);
            var patient = new Patient(
                tenantId,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14));
            var linkedAccount = PatientPortalAccount.CreateForExistingPatient(
                patient,
                "linked.patient",
                "versioned-password-hash");
            var foreignAccount = PatientPortalAccount.CreateUnlinked(
                Guid.NewGuid(),
                "foreign.patient",
                "versioned-password-hash");

            Assert.Throws<InvalidOperationException>(() =>
                link.Consume(linkedAccount, CreatedAtUtc.AddMinutes(5)));
            Assert.Throws<InvalidOperationException>(() =>
                link.Consume(foreignAccount, CreatedAtUtc.AddMinutes(5)));
        }

        [Fact]
        public void AuditEntry_CopiesOwnershipWithoutTokenMaterial()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");
            var link = CreateLink(tenant.Id, branch);
            var actorId = Guid.NewGuid();

            var audit = new PatientIntakeAccessLinkAuditEntry(
                link,
                PatientIntakeAccessLinkAuditAction.Issued,
                PatientIntakeAccessLinkAuditActorType.StaffUser,
                actorId,
                CreatedAtUtc,
                "correlation-1");

            Assert.Equal(link.TenantId, audit.TenantId);
            Assert.Equal(link.BranchId, audit.BranchId);
            Assert.Equal(link.Id, audit.PatientIntakeAccessLinkId);
            Assert.Equal(actorId, audit.ActorId);
            Assert.DoesNotContain("Token", audit.GetType().GetProperties().Select(property => property.Name));
        }

        private static PatientIntakeAccessLink CreateLink(
            Guid tenantId,
            Branch? branch)
        {
            return new PatientIntakeAccessLink(
                tenantId,
                branch,
                PatientIntakeAccessLinkPurpose.NewPatientWaitingRoomRegistration,
                new string('a', 64),
                CreatedAtUtc,
                CreatedAtUtc.AddMinutes(30),
                Guid.NewGuid());
        }
    }
}
