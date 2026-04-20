using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class ClinicalDiagnosis : Entity<Guid>
    {
        internal const int DiagnosisTextMaxLength = 250;
        internal const int NotesMaxLength = 500;

        public Guid ClinicalRecordId { get; private set; }
        public ClinicalRecord ClinicalRecord { get; private set; } = null!;

        public string DiagnosisText { get; private set; } = string.Empty;
        public string? Notes { get; private set; }
        public ClinicalDiagnosisStatus Status { get; private set; } = ClinicalDiagnosisStatus.Active;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime? ResolvedAtUtc { get; private set; }
        public Guid? ResolvedByUserId { get; private set; }

        private ClinicalDiagnosis()
        {
        }

        internal ClinicalDiagnosis(Guid clinicalRecordId, string diagnosisText, string? notes, Guid createdByUserId)
        {
            if (clinicalRecordId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record reference is required.", nameof(clinicalRecordId));
            }

            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical diagnosis author is required.", nameof(createdByUserId));
            }

            Id = Guid.NewGuid();
            ClinicalRecordId = clinicalRecordId;
            DiagnosisText = NormalizeRequired(diagnosisText, nameof(diagnosisText), DiagnosisTextMaxLength);
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void Resolve(Guid resolvedByUserId)
        {
            if (resolvedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical diagnosis resolver is required.", nameof(resolvedByUserId));
            }

            if (Status == ClinicalDiagnosisStatus.Resolved)
            {
                throw new InvalidOperationException("The clinical diagnosis is already resolved.");
            }

            Status = ClinicalDiagnosisStatus.Resolved;
            ResolvedAtUtc = DateTime.UtcNow;
            ResolvedByUserId = resolvedByUserId;
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
