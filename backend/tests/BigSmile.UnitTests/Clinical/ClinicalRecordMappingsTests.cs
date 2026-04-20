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
    }
}
