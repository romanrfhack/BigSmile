using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class ClinicalNote : Entity<Guid>
    {
        internal const int NoteTextMaxLength = 2000;

        public Guid ClinicalRecordId { get; private set; }
        public ClinicalRecord ClinicalRecord { get; private set; } = null!;

        public string NoteText { get; private set; } = string.Empty;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private ClinicalNote()
        {
        }

        internal ClinicalNote(Guid clinicalRecordId, string noteText, Guid createdByUserId)
        {
            if (clinicalRecordId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record reference is required.", nameof(clinicalRecordId));
            }

            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical note author is required.", nameof(createdByUserId));
            }

            Id = Guid.NewGuid();
            ClinicalRecordId = clinicalRecordId;
            NoteText = NormalizeRequired(noteText, nameof(noteText), NoteTextMaxLength);
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = DateTime.UtcNow;
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
