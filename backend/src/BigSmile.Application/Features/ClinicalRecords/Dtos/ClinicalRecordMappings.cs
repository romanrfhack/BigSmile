using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.ClinicalRecords.Dtos
{
    internal static class ClinicalRecordMappings
    {
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
                clinicalRecord.CreatedAtUtc,
                clinicalRecord.CreatedByUserId,
                clinicalRecord.LastUpdatedAtUtc,
                clinicalRecord.LastUpdatedByUserId);
        }
    }
}
