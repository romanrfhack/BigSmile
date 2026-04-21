namespace BigSmile.Application.Features.TreatmentPlans.Dtos
{
    public sealed record TreatmentPlanItemDto(
        Guid ItemId,
        string Title,
        string? Category,
        int Quantity,
        string? Notes,
        string? ToothCode,
        string? SurfaceCode,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId);

    public sealed record TreatmentPlanDetailDto(
        Guid TreatmentPlanId,
        Guid PatientId,
        string Status,
        IReadOnlyList<TreatmentPlanItemDto> Items,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId);
}
