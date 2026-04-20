using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Clinical
{
    public class ClinicalRecordTests
    {
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
