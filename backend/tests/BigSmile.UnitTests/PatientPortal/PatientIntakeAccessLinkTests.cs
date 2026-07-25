using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAccessLinkTests
    {
        private static readonly DateTime CreatedAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Create_PreservesTenantAndOptionalBranchOwnership()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");
            var actorUserId = Guid.NewGuid();

            var accessLink = CreateAccessLink(tenant, branch, actorUserId);

            Assert.Equal(tenant.Id, accessLink.TenantId);
            Assert.Equal(branch.Id, accessLink.BranchId);
            Assert.Equal(actorUserId, accessLink.CreatedByUserId);
            Assert.Equal(CreatedAtUtc.AddMinutes(30), accessLink.ExpiresAtUtc);
            Assert.True(accessLink.CanBeConsumedAt(CreatedAtUtc.AddMinutes(1)));
            Assert.False(accessLink.IsExpiredAt(CreatedAtUtc.AddMinutes(29)));
            Assert.True(accessLink.IsExpiredAt(CreatedAtUtc.AddMinutes(30)));
        }

        [Fact]
        public void Create_RejectsCrossTenantAndInactiveBranch()
        {
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            var foreignBranch = tenantB.AddBranch("Foreign");

            Assert.Throws<InvalidOperationException>(() =>
                CreateAccessLink(tenantA, foreignBranch, Guid.NewGuid()));

            var inactiveBranch = tenantA.AddBranch("Inactive");
            inactiveBranch.Deactivate();
            Assert.Throws<InvalidOperationException>(() =>
                CreateAccessLink(tenantA, inactiveBranch, Guid.NewGuid()));
        }

        [Fact]
        public void Revoke_IsTerminalAndCannotOccurAfterExpiry()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var accessLink = CreateAccessLink(tenant, branch: null, Guid.NewGuid());
            var actorUserId = Guid.NewGuid();

            accessLink.Revoke(actorUserId, CreatedAtUtc.AddMinutes(5));

            Assert.Equal(CreatedAtUtc.AddMinutes(5), accessLink.RevokedAtUtc);
            Assert.Equal(actorUserId, accessLink.RevokedByUserId);
            Assert.False(accessLink.CanBeConsumedAt(CreatedAtUtc.AddMinutes(6)));
            Assert.Throws<InvalidOperationException>(() =>
                accessLink.Revoke(Guid.NewGuid(), CreatedAtUtc.AddMinutes(7)));

            var expiredLink = CreateAccessLink(tenant, branch: null, Guid.NewGuid());
            Assert.Throws<InvalidOperationException>(() =>
                expiredLink.Revoke(Guid.NewGuid(), expiredLink.ExpiresAtUtc));
        }

        [Fact]
        public void Consume_BindsExactlyOneUnlinkedAccountAndWaitingRoomIntake()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("Main");
            var account = PatientPortalAccount.CreateUnlinked(
                tenant.Id,
                "waiting.patient",
                "versioned-password-hash");
            var intake = PatientIntake.CreateForNewPatient(
                account,
                branch,
                CreatedAtUtc);
            var accessLink = CreateAccessLink(tenant, branch, Guid.NewGuid());

            accessLink.Consume(account, intake, CreatedAtUtc.AddMinutes(2));

            Assert.Equal(CreatedAtUtc.AddMinutes(2), accessLink.ConsumedAtUtc);
            Assert.Equal(account.Id, accessLink.ConsumedByPatientPortalAccountId);
            Assert.Equal(intake.Id, accessLink.PatientIntakeId);
            Assert.False(accessLink.CanBeConsumedAt(CreatedAtUtc.AddMinutes(3)));
            Assert.Throws<InvalidOperationException>(() =>
                accessLink.Consume(account, intake, CreatedAtUtc.AddMinutes(3)));
            Assert.Throws<InvalidOperationException>(() =>
                accessLink.Revoke(Guid.NewGuid(), CreatedAtUtc.AddMinutes(3)));
        }

        [Fact]
        public void Consume_RejectsLinkedAccountCrossTenantAndBranchMismatch()
        {
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var branchA = tenantA.AddBranch("Main A");
            var branchA2 = tenantA.AddBranch("Second A");
            var tenantB = new Tenant("Tenant B", "tenant-b");

            var linkedPatient = new Patient(
                tenantA.Id,
                "Ana",
                "Lopez",
                new DateOnly(1990, 1, 1));
            var linkedAccount = PatientPortalAccount.CreateForExistingPatient(
                linkedPatient,
                "linked.patient",
                "versioned-password-hash");
            var linkedIntake = PatientIntake.CreateForExistingPatient(
                linkedAccount,
                linkedPatient,
                branchA,
                CreatedAtUtc);
            var link = CreateAccessLink(tenantA, branchA, Guid.NewGuid());
            Assert.Throws<InvalidOperationException>(() =>
                link.Consume(linkedAccount, linkedIntake, CreatedAtUtc.AddMinutes(1)));

            var foreignAccount = PatientPortalAccount.CreateUnlinked(
                tenantB.Id,
                "foreign.patient",
                "versioned-password-hash");
            var foreignIntake = PatientIntake.CreateForNewPatient(
                foreignAccount,
                branch: null,
                CreatedAtUtc);
            Assert.Throws<InvalidOperationException>(() =>
                link.Consume(foreignAccount, foreignIntake, CreatedAtUtc.AddMinutes(1)));

            var account = PatientPortalAccount.CreateUnlinked(
                tenantA.Id,
                "branch.mismatch",
                "versioned-password-hash");
            var mismatchedIntake = PatientIntake.CreateForNewPatient(
                account,
                branchA2,
                CreatedAtUtc);
            Assert.Throws<InvalidOperationException>(() =>
                link.Consume(account, mismatchedIntake, CreatedAtUtc.AddMinutes(1)));
        }

        [Fact]
        public void AuditEntry_ContainsNoRawTokenOrTokenHashField()
        {
            var tenant = new Tenant("Tenant A", "tenant-a");
            var accessLink = CreateAccessLink(tenant, branch: null, Guid.NewGuid());
            var audit = new PatientIntakeAccessLinkAuditEntry(
                accessLink,
                PatientIntakeAccessLinkAuditAction.Issued,
                PatientIntakeAccessLinkAuditActorType.StaffUser,
                Guid.NewGuid(),
                CreatedAtUtc,
                "trace-1");

            Assert.Equal(accessLink.Id, audit.PatientIntakeAccessLinkId);
            Assert.Equal(tenant.Id, audit.TenantId);
            Assert.Equal(PatientIntakeAccessLinkAuditAction.Issued, audit.Action);
            Assert.DoesNotContain(
                typeof(PatientIntakeAccessLinkAuditEntry).GetProperties(),
                property => property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase));
        }

        private static PatientIntakeAccessLink CreateAccessLink(
            Tenant tenant,
            Branch? branch,
            Guid actorUserId)
        {
            return new PatientIntakeAccessLink(
                tenant,
                branch,
                new string('a', 64),
                CreatedAtUtc,
                CreatedAtUtc.AddMinutes(30),
                actorUserId);
        }
    }
}
