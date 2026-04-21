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
        private const string SnapshotInitializedSummary = "Clinical snapshot initialized";
        private const string MedicalBackgroundUpdatedSummary = "Medical background updated";
        private const string CurrentMedicationsUpdatedSummary = "Current medications updated";
        private const string AllergiesUpdatedSummary = "Current allergies updated";

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
        public ICollection<ClinicalDiagnosis> Diagnoses { get; private set; } = new List<ClinicalDiagnosis>();
        public ICollection<ClinicalSnapshotHistoryEntry> SnapshotHistory { get; private set; } = new List<ClinicalSnapshotHistoryEntry>();

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
            AddSnapshotHistoryEntry(
                ClinicalSnapshotHistoryEntryType.SnapshotInitialized,
                ClinicalSnapshotHistorySection.Initial,
                SnapshotInitializedSummary,
                createdByUserId);
        }

        public bool ApplySnapshot(
            string? medicalBackgroundSummary,
            string? currentMedicationsSummary,
            IEnumerable<ClinicalAllergyDraft>? allergies,
            Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            var normalizedMedicalBackgroundSummary = NormalizeOptional(
                medicalBackgroundSummary,
                nameof(medicalBackgroundSummary),
                SummaryMaxLength);
            var normalizedCurrentMedicationsSummary = NormalizeOptional(
                currentMedicationsSummary,
                nameof(currentMedicationsSummary),
                SummaryMaxLength);
            var normalizedAllergies = NormalizeAllergies(allergies);

            var medicalBackgroundChanged = !string.Equals(
                MedicalBackgroundSummary,
                normalizedMedicalBackgroundSummary,
                StringComparison.Ordinal);
            var currentMedicationsChanged = !string.Equals(
                CurrentMedicationsSummary,
                normalizedCurrentMedicationsSummary,
                StringComparison.Ordinal);
            var allergiesChanged = !AllergiesMatch(normalizedAllergies);

            if (!medicalBackgroundChanged && !currentMedicationsChanged && !allergiesChanged)
            {
                return false;
            }

            if (medicalBackgroundChanged)
            {
                MedicalBackgroundSummary = normalizedMedicalBackgroundSummary;
                AddSnapshotHistoryEntry(
                    ClinicalSnapshotHistoryEntryType.MedicalBackgroundUpdated,
                    ClinicalSnapshotHistorySection.MedicalBackground,
                    MedicalBackgroundUpdatedSummary,
                    updatedByUserId);
            }

            if (currentMedicationsChanged)
            {
                CurrentMedicationsSummary = normalizedCurrentMedicationsSummary;
                AddSnapshotHistoryEntry(
                    ClinicalSnapshotHistoryEntryType.CurrentMedicationsUpdated,
                    ClinicalSnapshotHistorySection.CurrentMedications,
                    CurrentMedicationsUpdatedSummary,
                    updatedByUserId);
            }

            if (allergiesChanged)
            {
                ReplaceAllergiesInternal(normalizedAllergies);
                AddSnapshotHistoryEntry(
                    ClinicalSnapshotHistoryEntryType.AllergiesUpdated,
                    ClinicalSnapshotHistorySection.Allergies,
                    AllergiesUpdatedSummary,
                    updatedByUserId);
            }

            Touch(updatedByUserId);
            return true;
        }

        public bool UpdateSnapshot(string? medicalBackgroundSummary, string? currentMedicationsSummary, Guid updatedByUserId)
        {
            return ApplySnapshot(medicalBackgroundSummary, currentMedicationsSummary, Allergies.Select(ToDraft).ToArray(), updatedByUserId);
        }

        public bool ReplaceAllergies(IEnumerable<ClinicalAllergyDraft>? allergies, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            var normalizedAllergies = NormalizeAllergies(allergies);
            if (AllergiesMatch(normalizedAllergies))
            {
                return false;
            }

            ReplaceAllergiesInternal(normalizedAllergies);
            AddSnapshotHistoryEntry(
                ClinicalSnapshotHistoryEntryType.AllergiesUpdated,
                ClinicalSnapshotHistorySection.Allergies,
                AllergiesUpdatedSummary,
                updatedByUserId);
            Touch(updatedByUserId);
            return true;
        }

        public ClinicalNote AddClinicalNote(string noteText, Guid createdByUserId)
        {
            var note = new ClinicalNote(Id, noteText, createdByUserId);
            Notes.Add(note);
            Touch(createdByUserId);

            return note;
        }

        public ClinicalDiagnosis AddDiagnosis(string diagnosisText, string? notes, Guid createdByUserId)
        {
            var diagnosis = new ClinicalDiagnosis(Id, diagnosisText, notes, createdByUserId);
            Diagnoses.Add(diagnosis);
            Touch(createdByUserId);

            return diagnosis;
        }

        public ClinicalDiagnosis ResolveDiagnosis(Guid diagnosisId, Guid resolvedByUserId)
        {
            if (diagnosisId == Guid.Empty)
            {
                throw new ArgumentException("Clinical diagnosis reference is required.", nameof(diagnosisId));
            }

            var diagnosis = Diagnoses.SingleOrDefault(entry => entry.Id == diagnosisId);
            if (diagnosis is null)
            {
                throw new InvalidOperationException("The requested diagnosis does not exist in the current clinical record.");
            }

            diagnosis.Resolve(resolvedByUserId);
            Touch(resolvedByUserId);

            return diagnosis;
        }

        private void ReplaceAllergiesInternal(IEnumerable<ClinicalAllergyDraft>? allergies)
        {
            Allergies.Clear();

            foreach (var allergy in NormalizeAllergies(allergies))
            {
                Allergies.Add(new ClinicalAllergyEntry(
                    Id,
                    allergy.Substance,
                    allergy.ReactionSummary,
                    allergy.Notes));
            }
        }

        private void Touch(Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
        }

        private void EnsureActor(Guid updatedByUserId)
        {
            if (updatedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical record actor is required.", nameof(updatedByUserId));
            }
        }

        private void AddSnapshotHistoryEntry(
            ClinicalSnapshotHistoryEntryType entryType,
            ClinicalSnapshotHistorySection section,
            string summary,
            Guid changedByUserId)
        {
            SnapshotHistory.Add(new ClinicalSnapshotHistoryEntry(
                Id,
                entryType,
                section,
                summary,
                changedByUserId));
        }

        private bool AllergiesMatch(IReadOnlyCollection<ClinicalAllergyDraft> normalizedAllergies)
        {
            var currentAllergies = Allergies
                .Select(allergy => new ClinicalAllergyDraft(
                    allergy.Substance,
                    allergy.ReactionSummary,
                    allergy.Notes))
                .OrderBy(allergy => allergy.Substance, StringComparer.OrdinalIgnoreCase)
                .ThenBy(allergy => allergy.ReactionSummary, StringComparer.Ordinal)
                .ThenBy(allergy => allergy.Notes, StringComparer.Ordinal)
                .ToList();

            if (currentAllergies.Count != normalizedAllergies.Count)
            {
                return false;
            }

            return currentAllergies
                .Zip(normalizedAllergies, (left, right) =>
                    string.Equals(left.Substance, right.Substance, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(left.ReactionSummary, right.ReactionSummary, StringComparison.Ordinal) &&
                    string.Equals(left.Notes, right.Notes, StringComparison.Ordinal))
                .All(isEqual => isEqual);
        }

        private static IReadOnlyCollection<ClinicalAllergyDraft> NormalizeAllergies(IEnumerable<ClinicalAllergyDraft>? allergies)
        {
            var normalizedAllergies = new List<ClinicalAllergyDraft>();
            var seenSubstances = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var allergy in allergies ?? Array.Empty<ClinicalAllergyDraft>())
            {
                var normalizedSubstance = ClinicalAllergyEntry.NormalizeSubstance(allergy.Substance);
                if (!seenSubstances.Add(normalizedSubstance))
                {
                    throw new InvalidOperationException("Clinical allergies cannot contain duplicate substances.");
                }

                normalizedAllergies.Add(new ClinicalAllergyDraft(
                    normalizedSubstance,
                    ClinicalAllergyEntry.NormalizeReactionSummary(allergy.ReactionSummary),
                    ClinicalAllergyEntry.NormalizeNotes(allergy.Notes)));
            }

            return normalizedAllergies
                .OrderBy(allergy => allergy.Substance, StringComparer.OrdinalIgnoreCase)
                .ThenBy(allergy => allergy.ReactionSummary, StringComparer.Ordinal)
                .ThenBy(allergy => allergy.Notes, StringComparer.Ordinal)
                .ToList();
        }

        private static ClinicalAllergyDraft ToDraft(ClinicalAllergyEntry allergy)
        {
            return new ClinicalAllergyDraft(allergy.Substance, allergy.ReactionSummary, allergy.Notes);
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
