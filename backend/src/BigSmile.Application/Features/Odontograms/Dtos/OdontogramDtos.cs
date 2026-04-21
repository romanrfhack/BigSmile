namespace BigSmile.Application.Features.Odontograms.Dtos
{
    public sealed record OdontogramSurfaceStateDto(
        string SurfaceCode,
        string Status,
        DateTime UpdatedAtUtc,
        Guid UpdatedByUserId,
        IReadOnlyList<OdontogramSurfaceFindingDto> Findings);

    public sealed record OdontogramSurfaceFindingDto(
        Guid FindingId,
        string FindingType,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId);

    public sealed record OdontogramSurfaceFindingHistoryEntryDto(
        string EntryType,
        string ToothCode,
        string SurfaceCode,
        string FindingType,
        DateTime ChangedAtUtc,
        Guid ChangedByUserId,
        string Summary,
        Guid? ReferenceFindingId);

    public sealed record OdontogramToothStateDto(
        string ToothCode,
        string Status,
        DateTime UpdatedAtUtc,
        Guid UpdatedByUserId,
        IReadOnlyList<OdontogramSurfaceStateDto> Surfaces);

    public sealed record OdontogramDetailDto(
        Guid OdontogramId,
        Guid PatientId,
        IReadOnlyList<OdontogramToothStateDto> Teeth,
        IReadOnlyList<OdontogramSurfaceFindingHistoryEntryDto> FindingsHistory,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId);
}
