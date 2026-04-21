namespace BigSmile.Application.Features.Odontograms.Dtos
{
    public sealed record OdontogramToothStateDto(
        string ToothCode,
        string Status,
        DateTime UpdatedAtUtc,
        Guid UpdatedByUserId);

    public sealed record OdontogramDetailDto(
        Guid OdontogramId,
        Guid PatientId,
        IReadOnlyList<OdontogramToothStateDto> Teeth,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId);
}
