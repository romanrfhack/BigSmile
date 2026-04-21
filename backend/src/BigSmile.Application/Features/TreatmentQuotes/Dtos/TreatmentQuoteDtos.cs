namespace BigSmile.Application.Features.TreatmentQuotes.Dtos
{
    public sealed record TreatmentQuoteItemDto(
        Guid QuoteItemId,
        Guid SourceTreatmentPlanItemId,
        string Title,
        string? Category,
        int Quantity,
        string? Notes,
        string? ToothCode,
        string? SurfaceCode,
        decimal UnitPrice,
        decimal LineTotal,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId);

    public sealed record TreatmentQuoteDetailDto(
        Guid TreatmentQuoteId,
        Guid PatientId,
        Guid TreatmentPlanId,
        string Status,
        string CurrencyCode,
        decimal Total,
        IReadOnlyList<TreatmentQuoteItemDto> Items,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId);
}
