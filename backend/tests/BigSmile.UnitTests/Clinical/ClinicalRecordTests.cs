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
        public void AddEncounter_AddsTenantAndPatientOwnedEncounterWithLinkedAppendOnlyNote()
        {
            var clinicalRecord = CreateClinicalRecord();
            var actorUserId = Guid.NewGuid();
            var occurredAtUtc = new DateTime(2026, 5, 12, 15, 30, 0, DateTimeKind.Utc);

            var encounter = clinicalRecord.AddEncounter(
                occurredAtUtc,
                "Tooth sensitivity.",
                ClinicalEncounterConsultationType.Treatment,
                36.7m,
                120,
                80,
                72.5m,
                168.0m,
                16,
                78,
                "Clinical encounter note.",
                actorUserId);

            Assert.Equal(clinicalRecord.TenantId, encounter.TenantId);
            Assert.Equal(clinicalRecord.PatientId, encounter.PatientId);
            Assert.Equal(clinicalRecord.Id, encounter.ClinicalRecordId);
            Assert.Equal(occurredAtUtc, encounter.OccurredAtUtc);
            Assert.Equal("Tooth sensitivity.", encounter.ChiefComplaint);
            Assert.Equal(ClinicalEncounterConsultationType.Treatment, encounter.ConsultationType);
            Assert.Equal(36.7m, encounter.TemperatureC);
            Assert.Equal(120, encounter.BloodPressureSystolic);
            Assert.Equal(80, encounter.BloodPressureDiastolic);
            Assert.Equal(72.5m, encounter.WeightKg);
            Assert.Equal(168.0m, encounter.HeightCm);
            Assert.Equal(16, encounter.RespiratoryRatePerMinute);
            Assert.Equal(78, encounter.HeartRateBpm);
            Assert.Equal(actorUserId, encounter.CreatedByUserId);
            var note = Assert.Single(clinicalRecord.Notes);
            Assert.Equal(note.Id, encounter.ClinicalNoteId);
            Assert.Same(note, encounter.ClinicalNote);
            Assert.Equal("Clinical encounter note.", note.NoteText);
        }

        [Fact]
        public void AddEncounter_RejectsInvalidConsultationType()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentException>(() => clinicalRecord.AddEncounter(
                DateTime.UtcNow,
                "Pain.",
                (ClinicalEncounterConsultationType)999,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Guid.NewGuid()));

            Assert.Contains("consultation type is not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddEncounter_RejectsInvalidVitals()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => clinicalRecord.AddEncounter(
                DateTime.UtcNow,
                "Pain.",
                ClinicalEncounterConsultationType.Urgency,
                52.0m,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                Guid.NewGuid()));

            Assert.Contains("temperatureC", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AddEncounter_DoesNotAddLinkedNoteWhenEncounterValidationFails()
        {
            var clinicalRecord = CreateClinicalRecord();

            Assert.Throws<ArgumentOutOfRangeException>(() => clinicalRecord.AddEncounter(
                DateTime.UtcNow,
                "Pain.",
                ClinicalEncounterConsultationType.Urgency,
                52.0m,
                null,
                null,
                null,
                null,
                null,
                null,
                "Should not be added.",
                Guid.NewGuid()));

            Assert.Empty(clinicalRecord.Notes);
            Assert.Empty(clinicalRecord.Encounters);
        }

        [Fact]
        public void AddEncounter_RejectsIncompleteBloodPressure()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentException>(() => clinicalRecord.AddEncounter(
                DateTime.UtcNow,
                "Pain.",
                ClinicalEncounterConsultationType.Urgency,
                null,
                120,
                null,
                null,
                null,
                null,
                null,
                null,
                Guid.NewGuid()));

            Assert.Contains("requires both systolic and diastolic", exception.Message, StringComparison.OrdinalIgnoreCase);
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
            clinicalRecord.AddEncounter(
                DateTime.UtcNow,
                "Routine consultation.",
                ClinicalEncounterConsultationType.Treatment,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                actorUserId);

            Assert.Equal(originalTenantId, clinicalRecord.TenantId);
            Assert.Equal(originalPatientId, clinicalRecord.PatientId);
            Assert.Equal("History of bruxism.", clinicalRecord.MedicalBackgroundSummary);
            Assert.Equal("Ibuprofen as needed.", clinicalRecord.CurrentMedicationsSummary);
            Assert.Single(clinicalRecord.Allergies);
            Assert.Equal("Latex", clinicalRecord.Allergies.First().Substance);
            Assert.Single(clinicalRecord.Diagnoses);
            Assert.Single(clinicalRecord.Encounters);
            Assert.Contains(clinicalRecord.SnapshotHistory, entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.SnapshotInitialized);
        }

        [Fact]
        public void UpsertMedicalAnswers_AddsQuestionnaireAnswersWithTenantAndPatientOwnership()
        {
            var clinicalRecord = CreateClinicalRecord();
            var actorUserId = Guid.NewGuid();

            var changed = clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("currentMedicalTreatment", ClinicalMedicalAnswerValue.Yes, " Orthodontic follow-up. ")
                },
                actorUserId);

            Assert.True(changed);
            var answer = Assert.Single(clinicalRecord.MedicalAnswers);
            Assert.Equal(clinicalRecord.TenantId, answer.TenantId);
            Assert.Equal(clinicalRecord.PatientId, answer.PatientId);
            Assert.Equal(clinicalRecord.Id, answer.ClinicalRecordId);
            Assert.Equal("currentMedicalTreatment", answer.QuestionKey);
            Assert.Equal(ClinicalMedicalAnswerValue.Yes, answer.Answer);
            Assert.Equal("Orthodontic follow-up.", answer.Details);
            Assert.Equal(actorUserId, answer.UpdatedByUserId);
        }

        [Fact]
        public void UpsertMedicalAnswers_UpdatesExistingQuestionByKey()
        {
            var clinicalRecord = CreateClinicalRecord();
            var createdByUserId = Guid.NewGuid();
            var updatedByUserId = Guid.NewGuid();
            clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("regularMedication", ClinicalMedicalAnswerValue.Unknown, null)
                },
                createdByUserId);
            var answerId = clinicalRecord.MedicalAnswers.Single().Id;

            var changed = clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("regularMedication", ClinicalMedicalAnswerValue.No, "None.")
                },
                updatedByUserId);

            Assert.True(changed);
            var answer = Assert.Single(clinicalRecord.MedicalAnswers);
            Assert.Equal(answerId, answer.Id);
            Assert.Equal(ClinicalMedicalAnswerValue.No, answer.Answer);
            Assert.Equal("None.", answer.Details);
            Assert.Equal(updatedByUserId, answer.UpdatedByUserId);
        }

        [Fact]
        public void UpsertMedicalAnswers_RejectsInvalidQuestionKey()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentException>(() => clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("unknownQuestion", ClinicalMedicalAnswerValue.Yes, null)
                },
                Guid.NewGuid()));

            Assert.Contains("question key is not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpsertMedicalAnswers_RejectsInvalidAnswer()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<ArgumentException>(() => clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("diabetes", (ClinicalMedicalAnswerValue)999, null)
                },
                Guid.NewGuid()));

            Assert.Contains("answer is not supported", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpsertMedicalAnswers_RejectsDuplicateQuestionKeys()
        {
            var clinicalRecord = CreateClinicalRecord();

            var exception = Assert.Throws<InvalidOperationException>(() => clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("diabetes", ClinicalMedicalAnswerValue.Yes, null),
                    new ClinicalMedicalAnswerDraft(" diabetes ", ClinicalMedicalAnswerValue.No, null)
                },
                Guid.NewGuid()));

            Assert.Contains("duplicate question keys", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpsertMedicalAnswers_DoesNotChangeTenantPatientOrClinicalRecordOwnership()
        {
            var clinicalRecord = CreateClinicalRecord();
            var originalTenantId = clinicalRecord.TenantId;
            var originalPatientId = clinicalRecord.PatientId;
            var originalClinicalRecordId = clinicalRecord.Id;

            clinicalRecord.UpsertMedicalAnswers(
                new[]
                {
                    new ClinicalMedicalAnswerDraft("hypertension", ClinicalMedicalAnswerValue.No, null)
                },
                Guid.NewGuid());

            var answer = Assert.Single(clinicalRecord.MedicalAnswers);
            Assert.Equal(originalTenantId, clinicalRecord.TenantId);
            Assert.Equal(originalPatientId, clinicalRecord.PatientId);
            Assert.Equal(originalClinicalRecordId, clinicalRecord.Id);
            Assert.Equal(originalTenantId, answer.TenantId);
            Assert.Equal(originalPatientId, answer.PatientId);
            Assert.Equal(originalClinicalRecordId, answer.ClinicalRecordId);
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
