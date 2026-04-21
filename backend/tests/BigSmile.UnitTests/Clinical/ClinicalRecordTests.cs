using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Clinical
{
    public class ClinicalRecordTests
    {
        [Fact]
        public void CreateClinicalRecord_RegistersInitialSnapshotHistoryEntry()
        {
            var actorUserId = Guid.NewGuid();

            var clinicalRecord = new ClinicalRecord(
                Guid.NewGuid(),
                Guid.NewGuid(),
                actorUserId,
                "No relevant chronic conditions.",
                "None reported.");

            var historyEntry = Assert.Single(clinicalRecord.SnapshotHistory);
            Assert.Equal(ClinicalSnapshotHistoryEntryType.SnapshotInitialized, historyEntry.EntryType);
            Assert.Equal(ClinicalSnapshotHistorySection.Initial, historyEntry.Section);
            Assert.Equal("Clinical snapshot initialized", historyEntry.Summary);
            Assert.Equal(actorUserId, historyEntry.ChangedByUserId);
        }

        [Fact]
        public void ApplySnapshot_RegistersMedicalBackgroundHistoryWhenChanged()
        {
            var clinicalRecord = CreateClinicalRecord();

            var changed = clinicalRecord.ApplySnapshot(
                "Updated medical background.",
                clinicalRecord.CurrentMedicationsSummary,
                Array.Empty<ClinicalAllergyDraft>(),
                Guid.NewGuid());

            Assert.True(changed);
            Assert.Contains(
                clinicalRecord.SnapshotHistory,
                entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.MedicalBackgroundUpdated
                    && entry.Section == ClinicalSnapshotHistorySection.MedicalBackground
                    && entry.Summary == "Medical background updated");
        }

        [Fact]
        public void ApplySnapshot_RegistersCurrentMedicationsHistoryWhenChanged()
        {
            var clinicalRecord = CreateClinicalRecord();

            var changed = clinicalRecord.ApplySnapshot(
                clinicalRecord.MedicalBackgroundSummary,
                "Updated medications.",
                Array.Empty<ClinicalAllergyDraft>(),
                Guid.NewGuid());

            Assert.True(changed);
            Assert.Contains(
                clinicalRecord.SnapshotHistory,
                entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.CurrentMedicationsUpdated
                    && entry.Section == ClinicalSnapshotHistorySection.CurrentMedications
                    && entry.Summary == "Current medications updated");
        }

        [Fact]
        public void ReplaceAllergies_RejectsDuplicateSubstances()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<InvalidOperationException>(() => clinicalRecord.ReplaceAllergies(
                new[]
                {
                    new ClinicalAllergyDraft("Latex", "Rash", null),
                    new ClinicalAllergyDraft(" latex ", "Swelling", null)
                },
                Guid.NewGuid()));

            Assert.Contains("duplicate substances", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ReplaceAllergies_RegistersHistoryWhenChanged()
        {
            var clinicalRecord = CreateClinicalRecord();

            var changed = clinicalRecord.ReplaceAllergies(
                new[]
                {
                    new ClinicalAllergyDraft("Latex", "Rash", "Use nitrile gloves.")
                },
                Guid.NewGuid());

            Assert.True(changed);
            Assert.Contains(
                clinicalRecord.SnapshotHistory,
                entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.AllergiesUpdated
                    && entry.Section == ClinicalSnapshotHistorySection.Allergies
                    && entry.Summary == "Current allergies updated");
        }

        [Fact]
        public void ApplySnapshot_DoesNotRegisterHistoryForNoOpUpdate()
        {
            var actorUserId = Guid.NewGuid();
            var clinicalRecord = new ClinicalRecord(
                Guid.NewGuid(),
                Guid.NewGuid(),
                actorUserId,
                "No relevant chronic conditions.",
                "None reported.",
                new[]
                {
                    new ClinicalAllergyDraft("Latex", "Rash", "Use nitrile gloves.")
                });
            var initialHistoryCount = clinicalRecord.SnapshotHistory.Count;
            var originalLastUpdatedAtUtc = clinicalRecord.LastUpdatedAtUtc;
            var originalLastUpdatedByUserId = clinicalRecord.LastUpdatedByUserId;

            var changed = clinicalRecord.ApplySnapshot(
                "No relevant chronic conditions.",
                "None reported.",
                new[]
                {
                    new ClinicalAllergyDraft(" latex ", "Rash", "Use nitrile gloves.")
                },
                Guid.NewGuid());

            Assert.False(changed);
            Assert.Equal(initialHistoryCount, clinicalRecord.SnapshotHistory.Count);
            Assert.Equal(originalLastUpdatedAtUtc, clinicalRecord.LastUpdatedAtUtc);
            Assert.Equal(originalLastUpdatedByUserId, clinicalRecord.LastUpdatedByUserId);
        }

        [Fact]
        public void AddClinicalNote_RejectsEmptyText()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentException>(() => clinicalRecord.AddClinicalNote("   ", Guid.NewGuid()));

            Assert.Contains("noteText is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddDiagnosis_RejectsEmptyText()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentException>(() => clinicalRecord.AddDiagnosis("   ", null, Guid.NewGuid()));

            Assert.Contains("diagnosisText is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddDiagnosis_StartsAsActive()
        {
            var clinicalRecord = CreateClinicalRecord();

            var diagnosis = clinicalRecord.AddDiagnosis("Occlusal caries", "Upper molar.", Guid.NewGuid());

            Assert.Equal(ClinicalDiagnosisStatus.Active, diagnosis.Status);
            Assert.Null(diagnosis.ResolvedAtUtc);
            Assert.Null(diagnosis.ResolvedByUserId);
        }

        [Fact]
        public void ResolveDiagnosis_RejectsResolvingTwice()
        {
            var clinicalRecord = CreateClinicalRecord();
            var diagnosis = clinicalRecord.AddDiagnosis("Occlusal caries", null, Guid.NewGuid());
            clinicalRecord.ResolveDiagnosis(diagnosis.Id, Guid.NewGuid());

            var exception = Assert.Throws<InvalidOperationException>(() => clinicalRecord.ResolveDiagnosis(diagnosis.Id, Guid.NewGuid()));

            Assert.Contains("already resolved", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ResolveDiagnosis_RejectsMissingDiagnosis()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<InvalidOperationException>(() => clinicalRecord.ResolveDiagnosis(Guid.NewGuid(), Guid.NewGuid()));

            Assert.Contains("does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void SnapshotMutations_DoNotChangeTenantOrPatientOwnership()
        {
            var clinicalRecord = CreateClinicalRecord();
            var originalTenantId = clinicalRecord.TenantId;
            var originalPatientId = clinicalRecord.PatientId;
            var actorUserId = Guid.NewGuid();

            clinicalRecord.UpdateSnapshot("History of bruxism.", "Ibuprofen as needed.", actorUserId);
            clinicalRecord.ReplaceAllergies(
                new[] { new ClinicalAllergyDraft("Latex", "Rash", "Use nitrile gloves.") },
                actorUserId);
            clinicalRecord.AddClinicalNote("Initial clinical intake completed.", actorUserId);
            clinicalRecord.AddDiagnosis("Occlusal caries", null, actorUserId);

            Assert.Equal(originalTenantId, clinicalRecord.TenantId);
            Assert.Equal(originalPatientId, clinicalRecord.PatientId);
            Assert.Equal("History of bruxism.", clinicalRecord.MedicalBackgroundSummary);
            Assert.Equal("Ibuprofen as needed.", clinicalRecord.CurrentMedicationsSummary);
            Assert.Single(clinicalRecord.Allergies);
            Assert.Equal("Latex", clinicalRecord.Allergies.First().Substance);
            Assert.Single(clinicalRecord.Diagnoses);
            Assert.Contains(clinicalRecord.SnapshotHistory, entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.SnapshotInitialized);
        }

        private static ClinicalRecord CreateClinicalRecord()
        {
            return new ClinicalRecord(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "No relevant chronic conditions.",
                "None reported.");
        }
    }
}
