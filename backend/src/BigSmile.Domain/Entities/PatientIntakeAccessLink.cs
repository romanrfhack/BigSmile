using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientIntakeAccessLink : Entity<Guid>, ITenantOwnedEntity
    {
        public const int TokenHashMinLength = 32;
        public const int TokenHashMaxLength = 128;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid? BranchId { get; private set; }
        public Branch? Branch { get; private set; }

        public string TokenHash { get; private set; } = string.Empty;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }

        public DateTime? RevokedAtUtc { get; private set; }
        public Guid? RevokedByUserId { get; private set; }

        public DateTime? ConsumedAtUtc { get; private set; }
        public Guid? ConsumedByPatientPortalAccountId { get; private set; }
        public PatientPortalAccount? ConsumedByPatientPortalAccount { get; private set; }
        public Guid? PatientIntakeId { get; private set; }
        public PatientIntake? PatientIntake { get; private set; }

        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        private PatientIntakeAccessLink()
        {
        }

        public PatientIntakeAccessLink(
            Tenant tenant,
            Branch? branch,
            string tokenHash,
            DateTime createdAtUtc,
            DateTime expiresAtUtc,
            Guid createdByUserId)
        {
            ArgumentNullException.ThrowIfNull(tenant);
            EnsureUtc(createdAtUtc, nameof(createdAtUtc));
            EnsureUtc(expiresAtUtc, nameof(expiresAtUtc));
            EnsureActor(createdByUserId, nameof(createdByUserId));

            if (!tenant.IsActive)
            {
                throw new InvalidOperationException(
                    "An inactive tenant cannot issue a patient intake access link.");
            }

            EnsureBranchOwnership(branch, tenant.Id);

            if (expiresAtUtc <= createdAtUtc)
            {
                throw new ArgumentException(
                    "Patient intake access link expiry must be after creation time.",
                    nameof(expiresAtUtc));
            }

            Id = Guid.NewGuid();
            Tenant = tenant;
            TenantId = tenant.Id;
            Branch = branch;
            BranchId = branch?.Id;
            TokenHash = NormalizeTokenHash(tokenHash);
            CreatedAtUtc = createdAtUtc;
            CreatedByUserId = createdByUserId;
            ExpiresAtUtc = expiresAtUtc;
        }

        public bool CanBeConsumedAt(DateTime utcNow)
        {
            EnsureUtc(utcNow, nameof(utcNow));
            return utcNow >= CreatedAtUtc &&
                   utcNow < ExpiresAtUtc &&
                   !RevokedAtUtc.HasValue &&
                   !ConsumedAtUtc.HasValue;
        }

        public bool IsExpiredAt(DateTime utcNow)
        {
            EnsureUtc(utcNow, nameof(utcNow));
            return utcNow >= ExpiresAtUtc;
        }

        public void Revoke(Guid revokedByUserId, DateTime revokedAtUtc)
        {
            EnsureActor(revokedByUserId, nameof(revokedByUserId));
            EnsureUtc(revokedAtUtc, nameof(revokedAtUtc));

            if (ConsumedAtUtc.HasValue)
            {
                throw new InvalidOperationException(
                    "A consumed patient intake access link cannot be revoked.");
            }

            if (RevokedAtUtc.HasValue)
            {
                throw new InvalidOperationException(
                    "Patient intake access link has already been revoked.");
            }

            if (revokedAtUtc < CreatedAtUtc)
            {
                throw new ArgumentException(
                    "Patient intake access link cannot be revoked before it was created.",
                    nameof(revokedAtUtc));
            }

            if (revokedAtUtc >= ExpiresAtUtc)
            {
                throw new InvalidOperationException(
                    "An expired patient intake access link cannot be revoked.");
            }

            RevokedAtUtc = revokedAtUtc;
            RevokedByUserId = revokedByUserId;
        }

        public void Consume(
            PatientPortalAccount account,
            PatientIntake intake,
            DateTime consumedAtUtc)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(intake);
            EnsureUtc(consumedAtUtc, nameof(consumedAtUtc));

            if (!CanBeConsumedAt(consumedAtUtc))
            {
                throw new InvalidOperationException(
                    "Patient intake access link is not available for consumption.");
            }

            if (account.TenantId != TenantId || account.PatientId.HasValue)
            {
                throw new InvalidOperationException(
                    "Patient intake access link requires an unlinked account in the same tenant.");
            }

            if (intake.TenantId != TenantId ||
                intake.PatientPortalAccountId != account.Id ||
                intake.PatientId.HasValue ||
                intake.Origin != PatientIntakeOrigin.NewPatientWaitingRoom)
            {
                throw new InvalidOperationException(
                    "Patient intake access link can only be consumed by its same-tenant unlinked account and waiting-room intake.");
            }

            if (intake.BranchId != BranchId)
            {
                throw new InvalidOperationException(
                    "Patient intake access link and intake must use the same Branch context.");
            }

            ConsumedAtUtc = consumedAtUtc;
            ConsumedByPatientPortalAccountId = account.Id;
            ConsumedByPatientPortalAccount = account;
            PatientIntakeId = intake.Id;
            PatientIntake = intake;
        }

        private static string NormalizeTokenHash(string? tokenHash)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new ArgumentException(
                    "Patient intake access link token hash is required.",
                    nameof(tokenHash));
            }

            var normalized = tokenHash.Trim();
            if (normalized.Length is < TokenHashMinLength or > TokenHashMaxLength)
            {
                throw new ArgumentException(
                    $"Patient intake access link token hash must contain between {TokenHashMinLength} and {TokenHashMaxLength} characters.",
                    nameof(tokenHash));
            }

            return normalized;
        }

        private static void EnsureBranchOwnership(Branch? branch, Guid tenantId)
        {
            if (branch is null)
            {
                return;
            }

            if (branch.TenantId != tenantId)
            {
                throw new InvalidOperationException(
                    "Patient intake access link Branch must belong to the same tenant.");
            }

            if (!branch.IsActive)
            {
                throw new InvalidOperationException(
                    "An inactive Branch cannot be used for a patient intake access link.");
            }
        }

        private static void EnsureActor(Guid actorId, string paramName)
        {
            if (actorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Patient intake access link actor is required.",
                    paramName);
            }
        }

        private static void EnsureUtc(DateTime value, string paramName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient intake access link timestamps must be UTC.",
                    paramName);
            }
        }
    }
}
