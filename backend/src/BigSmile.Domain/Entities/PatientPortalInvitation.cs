using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientPortalInvitation : Entity<Guid>, ITenantOwnedEntity
    {
        public const int TokenHashMinLength = 32;
        public const int TokenHashMaxLength = 128;
        public const int PurposeMaxLength = 40;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public PatientPortalInvitationPurpose Purpose { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime ExpiresAtUtc { get; private set; }
        public DateTime? RevokedAtUtc { get; private set; }
        public Guid? RevokedByUserId { get; private set; }
        public DateTime? ConsumedAtUtc { get; private set; }
        public Guid? ConsumedByPatientPortalAccountId { get; private set; }
        public PatientPortalAccount? ConsumedByPatientPortalAccount { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        private PatientPortalInvitation()
        {
        }

        public PatientPortalInvitation(
            Guid tenantId,
            Guid patientId,
            PatientPortalInvitationPurpose purpose,
            string tokenHash,
            DateTime createdAtUtc,
            DateTime expiresAtUtc,
            Guid createdByUserId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal invitation tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal invitation patient reference is required.", nameof(patientId));
            }

            EnsureDefinedPurpose(purpose);
            EnsureActor(createdByUserId, nameof(createdByUserId));
            EnsureUtc(createdAtUtc, nameof(createdAtUtc));
            EnsureUtc(expiresAtUtc, nameof(expiresAtUtc));

            if (expiresAtUtc <= createdAtUtc)
            {
                throw new ArgumentException("Patient portal invitation expiry must be after creation time.", nameof(expiresAtUtc));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            Purpose = purpose;
            TokenHash = NormalizeTokenHash(tokenHash);
            CreatedAtUtc = createdAtUtc;
            CreatedByUserId = createdByUserId;
            ExpiresAtUtc = expiresAtUtc;
        }

        public bool CanBeConsumedAt(DateTime utcNow)
        {
            EnsureUtc(utcNow, nameof(utcNow));
            return !RevokedAtUtc.HasValue && !ConsumedAtUtc.HasValue && utcNow < ExpiresAtUtc;
        }

        public bool IsExpiredAt(DateTime utcNow)
        {
            EnsureUtc(utcNow, nameof(utcNow));
            return utcNow >= ExpiresAtUtc;
        }

        public void Consume(Guid patientPortalAccountId, DateTime consumedAtUtc)
        {
            if (patientPortalAccountId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal invitation consumer account is required.", nameof(patientPortalAccountId));
            }

            EnsureUtc(consumedAtUtc, nameof(consumedAtUtc));

            if (RevokedAtUtc.HasValue)
            {
                throw new InvalidOperationException("Patient portal invitation has been revoked.");
            }

            if (ConsumedAtUtc.HasValue)
            {
                throw new InvalidOperationException("Patient portal invitation has already been consumed.");
            }

            if (consumedAtUtc < CreatedAtUtc)
            {
                throw new ArgumentException("Patient portal invitation cannot be consumed before it was created.", nameof(consumedAtUtc));
            }

            if (consumedAtUtc >= ExpiresAtUtc)
            {
                throw new InvalidOperationException("Patient portal invitation has expired.");
            }

            ConsumedAtUtc = consumedAtUtc;
            ConsumedByPatientPortalAccountId = patientPortalAccountId;
        }

        public void Revoke(Guid revokedByUserId, DateTime revokedAtUtc)
        {
            EnsureActor(revokedByUserId, nameof(revokedByUserId));
            EnsureUtc(revokedAtUtc, nameof(revokedAtUtc));

            if (ConsumedAtUtc.HasValue)
            {
                throw new InvalidOperationException("A consumed patient portal invitation cannot be revoked.");
            }

            if (RevokedAtUtc.HasValue)
            {
                throw new InvalidOperationException("Patient portal invitation has already been revoked.");
            }

            if (revokedAtUtc < CreatedAtUtc)
            {
                throw new ArgumentException("Patient portal invitation cannot be revoked before it was created.", nameof(revokedAtUtc));
            }

            RevokedAtUtc = revokedAtUtc;
            RevokedByUserId = revokedByUserId;
        }

        private static string NormalizeTokenHash(string? tokenHash)
        {
            if (string.IsNullOrWhiteSpace(tokenHash))
            {
                throw new ArgumentException("Patient portal invitation token hash is required.", nameof(tokenHash));
            }

            var normalized = tokenHash.Trim();
            if (normalized.Length is < TokenHashMinLength or > TokenHashMaxLength)
            {
                throw new ArgumentException(
                    $"Patient portal invitation token hash must contain between {TokenHashMinLength} and {TokenHashMaxLength} characters.",
                    nameof(tokenHash));
            }

            return normalized;
        }

        private static void EnsureDefinedPurpose(PatientPortalInvitationPurpose purpose)
        {
            if (!Enum.IsDefined(purpose))
            {
                throw new ArgumentException("Patient portal invitation purpose is not supported.", nameof(purpose));
            }
        }

        private static void EnsureActor(Guid actorUserId, string paramName)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal invitation actor is required.", paramName);
            }
        }

        private static void EnsureUtc(DateTime value, string paramName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Patient portal invitation timestamps must be UTC.", paramName);
            }
        }
    }
}
