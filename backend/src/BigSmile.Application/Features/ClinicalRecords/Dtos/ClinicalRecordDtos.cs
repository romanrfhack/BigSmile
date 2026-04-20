namespace BigSmile.Application.Features.ClinicalRecords.Dtos
{
    public sealed record ClinicalAllergyEntryDto(
        Guid Id,
        string Substance,
        string? ReactionSummary,
        string? Notes);

    public sealed record ClinicalNoteDto(
        Guid Id,
        string NoteText,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId);

    public sealed record ClinicalDiagnosisDto(
        Guid DiagnosisId,
        string DiagnosisText,
        string? Notes,
        string Status,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime? ResolvedAtUtc,
        Guid? ResolvedByUserId);

    public sealed record ClinicalRecordDetailDto(
        Guid ClinicalRecordId,
        Guid PatientId,
        string? MedicalBackgroundSummary,
        string? CurrentMedicationsSummary,
        IReadOnlyList<ClinicalAllergyEntryDto> Allergies,
        IReadOnlyList<ClinicalNoteDto> Notes,
        IReadOnlyList<ClinicalDiagnosisDto> Diagnoses,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId);
}
