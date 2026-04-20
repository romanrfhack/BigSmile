using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class ClinicalAllergyEntry : Entity<Guid>
    {
        internal const int SubstanceMaxLength = 150;
        internal const int ReactionSummaryMaxLength = 500;
        internal const int NotesMaxLength = 500;

        public Guid ClinicalRecordId { get; private set; }
        public ClinicalRecord ClinicalRecord { get; private set; } = null!;

        public string Substance { get; private set; } = string.Empty;
        public string? ReactionSummary { get; private set; }
        public string? Notes { get; private set; }

        private ClinicalAllergyEntry()
        {
        }

        internal ClinicalAllergyEntry(Guid clinicalRecordId, string substance, string? reactionSummary, string? notes)
        {
            if (clinicalRecordId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record reference is required.", nameof(clinicalRecordId));
            }

            Id = Guid.NewGuid();
            ClinicalRecordId = clinicalRecordId;
            Substance = NormalizeSubstance(substance);
            ReactionSummary = NormalizeOptional(reactionSummary, nameof(reactionSummary), ReactionSummaryMaxLength);
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
        }

        internal static string NormalizeSubstance(string substance)
        {
            return NormalizeRequired(substance, nameof(substance), SubstanceMaxLength);
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
