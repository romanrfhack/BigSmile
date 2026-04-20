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

            Assert.Equal(originalTenantId, clinicalRecord.TenantId);
            Assert.Equal(originalPatientId, clinicalRecord.PatientId);
            Assert.Equal("History of bruxism.", clinicalRecord.MedicalBackgroundSummary);
            Assert.Equal("Ibuprofen as needed.", clinicalRecord.CurrentMedicationsSummary);
            Assert.Single(clinicalRecord.Allergies);
            Assert.Equal("Latex", clinicalRecord.Allergies.First().Substance);
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
