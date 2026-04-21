using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.ClinicalRecords.Dtos
{
    internal static class ClinicalRecordMappings
    {
        private const string ClinicalNoteCreated = "ClinicalNoteCreated";
        private const string ClinicalDiagnosisCreated = "ClinicalDiagnosisCreated";
        private const string ClinicalDiagnosisResolved = "ClinicalDiagnosisResolved";

        public static ClinicalRecordDetailDto ToDetailDto(this ClinicalRecord clinicalRecord)
        {
            return new ClinicalRecordDetailDto(
                clinicalRecord.Id,
                clinicalRecord.PatientId,
                clinicalRecord.MedicalBackgroundSummary,
                clinicalRecord.CurrentMedicationsSummary,
                clinicalRecord.Allergies
                    .OrderBy(allergy => allergy.Substance)
                    .Select(allergy => new ClinicalAllergyEntryDto(
                        allergy.Id,
                        allergy.Substance,
                        allergy.ReactionSummary,
                        allergy.Notes))
                    .ToList(),
                clinicalRecord.Notes
                    .OrderByDescending(note => note.CreatedAtUtc)
                    .ThenByDescending(note => note.Id)
                    .Select(note => new ClinicalNoteDto(
                        note.Id,
                        note.NoteText,
                        note.CreatedAtUtc,
                        note.CreatedByUserId))
                    .ToList(),
                clinicalRecord.Diagnoses
                    .OrderBy(diagnosis => diagnosis.Status == ClinicalDiagnosisStatus.Active ? 0 : 1)
                    .ThenByDescending(diagnosis => diagnosis.CreatedAtUtc)
                    .ThenByDescending(diagnosis => diagnosis.Id)
                    .Select(diagnosis => new ClinicalDiagnosisDto(
                        diagnosis.Id,
                        diagnosis.DiagnosisText,
                        diagnosis.Notes,
                        diagnosis.Status.ToString(),
                        diagnosis.CreatedAtUtc,
                        diagnosis.CreatedByUserId,
                        diagnosis.ResolvedAtUtc,
                        diagnosis.ResolvedByUserId))
                    .ToList(),
                clinicalRecord.SnapshotHistory
                    .OrderByDescending(entry => entry.ChangedAtUtc)
                    .ThenByDescending(entry => entry.Id)
                    .Select(entry => new ClinicalSnapshotHistoryEntryDto(
                        entry.EntryType.ToString(),
                        entry.ChangedAtUtc,
                        entry.ChangedByUserId,
                        entry.Section.ToString(),
                        entry.Summary))
                    .ToList(),
                BuildTimeline(clinicalRecord),
                clinicalRecord.CreatedAtUtc,
                clinicalRecord.CreatedByUserId,
                clinicalRecord.LastUpdatedAtUtc,
                clinicalRecord.LastUpdatedByUserId);
        }

        private static IReadOnlyList<ClinicalTimelineEntryDto> BuildTimeline(ClinicalRecord clinicalRecord)
        {
            return clinicalRecord.Notes
                .Select(note => new ClinicalTimelineEntryDto(
                    ClinicalNoteCreated,
                    note.CreatedAtUtc,
                    note.CreatedByUserId,
                    "Clinical note added",
                    note.NoteText,
                    note.Id))
                .Concat(clinicalRecord.Diagnoses.Select(diagnosis => new ClinicalTimelineEntryDto(
                    ClinicalDiagnosisCreated,
                    diagnosis.CreatedAtUtc,
                    diagnosis.CreatedByUserId,
                    "Diagnosis added",
                    BuildDiagnosisSummary(diagnosis),
                    diagnosis.Id)))
                .Concat(clinicalRecord.Diagnoses
                    .Where(diagnosis => diagnosis.ResolvedAtUtc.HasValue)
                    .Select(diagnosis => new ClinicalTimelineEntryDto(
                        ClinicalDiagnosisResolved,
                        diagnosis.ResolvedAtUtc!.Value,
                        diagnosis.ResolvedByUserId ?? diagnosis.CreatedByUserId,
                        "Diagnosis resolved",
                        BuildDiagnosisSummary(diagnosis),
                        diagnosis.Id)))
                .OrderByDescending(entry => entry.OccurredAtUtc)
                .ThenByDescending(entry => entry.ReferenceId)
                .ThenByDescending(entry => entry.EventType, StringComparer.Ordinal)
                .ToList();
        }

        private static string BuildDiagnosisSummary(ClinicalDiagnosis diagnosis)
        {
            return string.IsNullOrWhiteSpace(diagnosis.Notes)
                ? diagnosis.DiagnosisText
                : $"{diagnosis.DiagnosisText}: {diagnosis.Notes}";
        }
    }
}
