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

        private static void SetCreatedAt(ClinicalDiagnosis diagnosis, DateTime value)
        {
            var field = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.CreatedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(diagnosis, value);
        }
    }
}
