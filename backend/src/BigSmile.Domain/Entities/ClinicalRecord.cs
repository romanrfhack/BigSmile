using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed record ClinicalAllergyDraft(
        string Substance,
        string? ReactionSummary,
        string? Notes);

    public sealed class ClinicalRecord : Entity<Guid>, ITenantOwnedEntity
    {
        private const int SummaryMaxLength = 2000;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public string? MedicalBackgroundSummary { get; private set; }
        public string? CurrentMedicationsSummary { get; private set; }

        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public Guid LastUpdatedByUserId { get; private set; }

        public ICollection<ClinicalAllergyEntry> Allergies { get; private set; } = new List<ClinicalAllergyEntry>();
        public ICollection<ClinicalNote> Notes { get; private set; } = new List<ClinicalNote>();

        private ClinicalRecord()
        {
        }

        public ClinicalRecord(
            Guid tenantId,
            Guid patientId,
            Guid createdByUserId,
            string? medicalBackgroundSummary = null,
            string? currentMedicationsSummary = null,
            IEnumerable<ClinicalAllergyDraft>? allergies = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record patient reference is required.", nameof(patientId));
            }

            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record author is required.", nameof(createdByUserId));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = DateTime.UtcNow;
            LastUpdatedAtUtc = CreatedAtUtc;
            LastUpdatedByUserId = createdByUserId;
            MedicalBackgroundSummary = NormalizeOptional(medicalBackgroundSummary, nameof(medicalBackgroundSummary), SummaryMaxLength);
            CurrentMedicationsSummary = NormalizeOptional(currentMedicationsSummary, nameof(currentMedicationsSummary), SummaryMaxLength);

            ReplaceAllergiesInternal(allergies);
        }

        public void UpdateSnapshot(string? medicalBackgroundSummary, string? currentMedicationsSummary, Guid updatedByUserId)
        {
            MedicalBackgroundSummary = NormalizeOptional(medicalBackgroundSummary, nameof(medicalBackgroundSummary), SummaryMaxLength);
            CurrentMedicationsSummary = NormalizeOptional(currentMedicationsSummary, nameof(currentMedicationsSummary), SummaryMaxLength);
            Touch(updatedByUserId);
        }

        public void ReplaceAllergies(IEnumerable<ClinicalAllergyDraft>? allergies, Guid updatedByUserId)
        {
            ReplaceAllergiesInternal(allergies);
            Touch(updatedByUserId);
        }

        public ClinicalNote AddClinicalNote(string noteText, Guid createdByUserId)
        {
            var note = new ClinicalNote(Id, noteText, createdByUserId);
            Notes.Add(note);
            Touch(createdByUserId);

            return note;
        }

        private void ReplaceAllergiesInternal(IEnumerable<ClinicalAllergyDraft>? allergies)
        {
            Allergies.Clear();

            var seenSubstances = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var allergy in allergies ?? Array.Empty<ClinicalAllergyDraft>())
            {
                var normalizedSubstance = ClinicalAllergyEntry.NormalizeSubstance(allergy.Substance);
                if (!seenSubstances.Add(normalizedSubstance))
                {
                    throw new InvalidOperationException("Clinical allergies cannot contain duplicate substances.");
                }

                Allergies.Add(new ClinicalAllergyEntry(
                    Id,
                    normalizedSubstance,
                    allergy.ReactionSummary,
                    allergy.Notes));
            }
        }

        private void Touch(Guid updatedByUserId)
        {
            if (updatedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record actor is required.", nameof(updatedByUserId));
            }

            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
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
