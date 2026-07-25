using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientIntakeRevision : Entity<Guid>, ITenantOwnedEntity
    {
        public const int CorrelationIdMaxLength = 100;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientIntakeId { get; private set; }
        public PatientIntake PatientIntake { get; private set; } = null!;

        public Guid ActorPatientPortalAccountId { get; private set; }
        public PatientPortalAccount ActorPatientPortalAccount { get; private set; } = null!;

        public int RevisionNumber { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string ChangedFieldsJson { get; private set; } = string.Empty;
        public string SnapshotJson { get; private set; } = string.Empty;
        public string CorrelationId { get; private set; } = string.Empty;

        private PatientIntakeRevision()
        {
        }

        internal PatientIntakeRevision(
            PatientIntake patientIntake,
            int revisionNumber,
            Guid actorPatientPortalAccountId,
            DateTime occurredAtUtc,
            string changedFieldsJson,
            string snapshotJson,
            string correlationId)
        {
            ArgumentNullException.ThrowIfNull(patientIntake);
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            if (revisionNumber <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(revisionNumber));
            }

            if (actorPatientPortalAccountId == Guid.Empty ||
                actorPatientPortalAccountId != patientIntake.PatientPortalAccountId)
            {
                throw new ArgumentException(
                    "Patient intake revision actor must be the owning portal account.",
                    nameof(actorPatientPortalAccountId));
            }

            Id = Guid.NewGuid();
            TenantId = patientIntake.TenantId;
            PatientIntakeId = patientIntake.Id;
            PatientIntake = patientIntake;
            ActorPatientPortalAccountId = actorPatientPortalAccountId;
            RevisionNumber = revisionNumber;
            OccurredAtUtc = occurredAtUtc;
            ChangedFieldsJson = NormalizeRequiredJson(changedFieldsJson, nameof(changedFieldsJson));
            SnapshotJson = NormalizeRequiredJson(snapshotJson, nameof(snapshotJson));
            CorrelationId = NormalizeRequired(
                correlationId,
                nameof(correlationId),
                CorrelationIdMaxLength);
        }

        private static string NormalizeRequiredJson(string? value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    "Patient intake revision JSON content is required.",
                    paramName);
            }

            return value.Trim();
        }

        private static string NormalizeRequired(
            string? value,
            string paramName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(
                    $"{paramName} is required.",
                    paramName);
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{paramName} exceeds the allowed length of {maxLength}.",
                    paramName);
            }

            return normalized;
        }

        private static void EnsureUtc(DateTime value, string paramName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient intake revision timestamps must be UTC.",
                    paramName);
            }
        }
    }
}
