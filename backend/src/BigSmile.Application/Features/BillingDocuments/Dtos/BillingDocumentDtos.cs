namespace BigSmile.Application.Features.BillingDocuments.Dtos
{
    public sealed record BillingDocumentItemDto(
        Guid BillingItemId,
        Guid SourceTreatmentQuoteItemId,
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

    public sealed record BillingDocumentDetailDto(
        Guid BillingDocumentId,
        Guid PatientId,
        Guid TreatmentQuoteId,
        string Status,
        string CurrencyCode,
        decimal TotalAmount,
        IReadOnlyList<BillingDocumentItemDto> Items,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime LastUpdatedAtUtc,
        Guid LastUpdatedByUserId,
        DateTime? IssuedAtUtc,
        Guid? IssuedByUserId);
}
