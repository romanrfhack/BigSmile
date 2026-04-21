using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class ClinicalSnapshotHistoryEntry : Entity<Guid>
    {
        internal const int SummaryMaxLength = 200;

        public Guid ClinicalRecordId { get; private set; }
        public ClinicalRecord ClinicalRecord { get; private set; } = null!;

        public ClinicalSnapshotHistoryEntryType EntryType { get; private set; }
        public ClinicalSnapshotHistorySection Section { get; private set; }
        public string Summary { get; private set; } = string.Empty;
        public DateTime ChangedAtUtc { get; private set; }
        public Guid ChangedByUserId { get; private set; }

        private ClinicalSnapshotHistoryEntry()
        {
        }

        internal ClinicalSnapshotHistoryEntry(
            Guid clinicalRecordId,
            ClinicalSnapshotHistoryEntryType entryType,
            ClinicalSnapshotHistorySection section,
            string summary,
            Guid changedByUserId)
        {
            if (clinicalRecordId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record reference is required.", nameof(clinicalRecordId));
            }

            if (changedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical snapshot history actor is required.", nameof(changedByUserId));
            }

            Id = Guid.NewGuid();
            ClinicalRecordId = clinicalRecordId;
            EntryType = entryType;
            Section = section;
            Summary = NormalizeRequired(summary, nameof(summary), SummaryMaxLength);
            ChangedAtUtc = DateTime.UtcNow;
            ChangedByUserId = changedByUserId;
        }

        private static string NormalizeRequired(string value, string paramName, int maxLength)
        {
            var normalized = NormalizeOptional(value, paramName, maxLength);
            if (normalized is null)
            {
                throw new ArgumentException($"{paramName} is required.", paramName);
            }

            return normalized;
        }

        private static string? NormalizeOptional(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} exceeds the allowed length of {maxLength}.", paramName);
            }

            return normalized;
        }
    }
}
