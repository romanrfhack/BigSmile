using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Clinical
{
    public class ClinicalRecordMappingsTests
    {
        [Fact]
        public void ToDetailDto_OrdersDiagnosesWithActiveFirstAndNewestFirstWithinEachGroup()
        {
            var clinicalRecord = new ClinicalRecord(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Background.",
                null);

            var activeOlder = clinicalRecord.AddDiagnosis("Active older", null, Guid.NewGuid());
            SetCreatedAt(activeOlder, new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc));

            var resolvedNewest = clinicalRecord.AddDiagnosis("Resolved newest", null, Guid.NewGuid());
            SetCreatedAt(resolvedNewest, new DateTime(2026, 4, 20, 13, 0, 0, DateTimeKind.Utc));
            clinicalRecord.ResolveDiagnosis(resolvedNewest.Id, Guid.NewGuid());

            var activeNewest = clinicalRecord.AddDiagnosis("Active newest", null, Guid.NewGuid());
            SetCreatedAt(activeNewest, new DateTime(2026, 4, 20, 12, 0, 0, DateTimeKind.Utc));

            var resolvedOlder = clinicalRecord.AddDiagnosis("Resolved older", null, Guid.NewGuid());
            SetCreatedAt(resolvedOlder, new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Utc));
            clinicalRecord.ResolveDiagnosis(resolvedOlder.Id, Guid.NewGuid());

            var dto = clinicalRecord.ToDetailDto();

            Assert.Equal(
                new[] { "Active newest", "Active older", "Resolved newest", "Resolved older" },
                dto.Diagnoses.Select(diagnosis => diagnosis.DiagnosisText).ToArray());
        }

        [Fact]
        public void ToDetailDto_BuildsClinicalTimelineNewestFirstFromNotesAndDiagnosesOnly()
        {
            var clinicalRecord = new ClinicalRecord(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Background.",
                "Current medications.");

            var note = clinicalRecord.AddClinicalNote("Clinical note summary.", Guid.NewGuid());
            SetCreatedAt(note, new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc));

            var createdDiagnosis = clinicalRecord.AddDiagnosis("Occlusal caries", "Upper molar.", Guid.NewGuid());
            SetCreatedAt(createdDiagnosis, new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc));

            var resolvedDiagnosis = clinicalRecord.AddDiagnosis("Gingivitis", null, Guid.NewGuid());
            SetCreatedAt(resolvedDiagnosis, new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Utc));
            clinicalRecord.ResolveDiagnosis(resolvedDiagnosis.Id, Guid.NewGuid());
            SetResolvedAt(resolvedDiagnosis, new DateTime(2026, 4, 20, 12, 0, 0, DateTimeKind.Utc), Guid.NewGuid());

            var dto = clinicalRecord.ToDetailDto();

            Assert.Equal(
                new[]
                {
                    "ClinicalDiagnosisResolved",
                    "ClinicalDiagnosisCreated",
                    "ClinicalDiagnosisCreated",
                    "ClinicalNoteCreated"
                },
                dto.Timeline.Select(entry => entry.EventType).ToArray());

            Assert.Equal(
                new[]
                {
                    "Diagnosis resolved",
                    "Diagnosis added",
                    "Diagnosis added",
                    "Clinical note added"
                },
                dto.Timeline.Select(entry => entry.Title).ToArray());

            Assert.Equal(
                new[]
                {
                    "Gingivitis",
                    "Gingivitis",
                    "Occlusal caries: Upper molar.",
                    "Clinical note summary."
                },
                dto.Timeline.Select(entry => entry.Summary).ToArray());
        }

        [Fact]
        public void ToDetailDto_OrdersSnapshotHistoryNewestFirst()
        {
            var actorUserId = Guid.NewGuid();
            var clinicalRecord = new ClinicalRecord(
                Guid.NewGuid(),
                Guid.NewGuid(),
                actorUserId,
                "Background.",
                null);

            SetChangedAt(
                clinicalRecord.SnapshotHistory.Single(),
                new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc));

            clinicalRecord.ApplySnapshot("Updated background.", null, Array.Empty<ClinicalAllergyDraft>(), actorUserId);
            SetChangedAt(
                clinicalRecord.SnapshotHistory.Single(entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.MedicalBackgroundUpdated),
                new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc));

            clinicalRecord.ApplySnapshot("Updated background.", "Ibuprofen.", Array.Empty<ClinicalAllergyDraft>(), actorUserId);
            SetChangedAt(
                clinicalRecord.SnapshotHistory.Single(entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.CurrentMedicationsUpdated),
                new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Utc));

            clinicalRecord.ReplaceAllergies(
                new[]
                {
                    new ClinicalAllergyDraft("Latex", "Rash", null)
                },
                actorUserId);
            SetChangedAt(
                clinicalRecord.SnapshotHistory.Single(entry => entry.EntryType == ClinicalSnapshotHistoryEntryType.AllergiesUpdated),
                new DateTime(2026, 4, 20, 12, 0, 0, DateTimeKind.Utc));

            var dto = clinicalRecord.ToDetailDto();

            Assert.Equal(
                new[]
                {
                    "AllergiesUpdated",
                    "CurrentMedicationsUpdated",
                    "MedicalBackgroundUpdated",
                    "SnapshotInitialized"
                },
                dto.SnapshotHistory.Select(entry => entry.EntryType).ToArray());

            Assert.Equal(
                new[]
                {
                    "Allergies",
                    "CurrentMedications",
                    "MedicalBackground",
                    "Initial"
                },
                dto.SnapshotHistory.Select(entry => entry.Section).ToArray());
        }

        private static void SetCreatedAt(ClinicalDiagnosis diagnosis, DateTime value)
        {
            var field = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.CreatedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(diagnosis, value);
        }

        private static void SetCreatedAt(ClinicalNote note, DateTime value)
        {
            var field = typeof(ClinicalNote)
                .GetField($"<{nameof(ClinicalNote.CreatedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(note, value);
        }

        private static void SetResolvedAt(ClinicalDiagnosis diagnosis, DateTime value, Guid resolvedByUserId)
        {
            var resolvedAtField = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.ResolvedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var resolvedByField = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.ResolvedByUserId)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            resolvedAtField.SetValue(diagnosis, value);
            resolvedByField.SetValue(diagnosis, resolvedByUserId);
        }

        private static void SetChangedAt(ClinicalSnapshotHistoryEntry historyEntry, DateTime value)
        {
            var field = typeof(ClinicalSnapshotHistoryEntry)
                .GetField($"<{nameof(ClinicalSnapshotHistoryEntry.ChangedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(historyEntry, value);
        }
    }
}
