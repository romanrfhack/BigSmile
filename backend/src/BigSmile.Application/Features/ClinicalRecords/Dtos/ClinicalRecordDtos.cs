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

    public sealed record ClinicalTimelineEntryDto(
        string EventType,
        DateTime OccurredAtUtc,
        Guid ActorUserId,
        string Title,
        string Summary,
        Guid ReferenceId);

    public sealed record ClinicalSnapshotHistoryEntryDto(
        string EntryType,
        DateTime ChangedAtUtc,
        Guid ChangedByUserId,
        string Section,
        string Summary);

    public sealed record ClinicalMedicalAnswerDto(
        Guid? Id,
        string QuestionKey,
        string Answer,
        string? Details,
        DateTime? UpdatedAtUtc,
        Guid? UpdatedByUserId);

    public sealed record ClinicalMedicalQuestionnaireDto(
        Guid ClinicalRecordId,
        Guid PatientId,
        IReadOnlyList<ClinicalMedicalAnswerDto> Answers);

    public sealed record ClinicalRecordDetailDto(
        Guid ClinicalRecordId,
        Guid PatientId,
        string? MedicalBackgroundSummary,
        string? CurrentMedicationsSummary,
        IReadOnlyList<ClinicalAllergyEntryDto> Allergies,
        IReadOnlyList<ClinicalNoteDto> Notes,
        IReadOnlyList<ClinicalDiagnosisDto> Diagnoses,
        IReadOnlyList<ClinicalSnapshotHistoryEntryDto> SnapshotHistory,
        IReadOnlyList<ClinicalTimelineEntryDto> Timeline,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId);
}
