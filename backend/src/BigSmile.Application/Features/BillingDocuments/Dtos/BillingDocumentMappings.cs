using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.BillingDocuments.Dtos
{
    internal static class BillingDocumentMappings
    {
        public static BillingDocumentDetailDto ToDetailDto(this BillingDocument billingDocument)
        {
            return new BillingDocumentDetailDto(
                billingDocument.Id,
                billingDocument.PatientId,
                billingDocument.TreatmentQuoteId,
                billingDocument.Status.ToString(),
                billingDocument.CurrencyCode,
                billingDocument.TotalAmount,
                billingDocument.Items
                    .OrderBy(item => item.CreatedAtUtc)
                    .ThenBy(item => item.Id)
                    .Select(item => new BillingDocumentItemDto(
                        item.Id,
                        item.SourceTreatmentQuoteItemId,
                        item.Title,
                        item.Category,
                        item.Quantity,
                        item.Notes,
                        item.ToothCode,
                        item.SurfaceCode,
                        item.UnitPrice,
                        item.LineTotal,
                        item.CreatedAtUtc,
                        item.CreatedByUserId))
                    .ToList(),
                billingDocument.CreatedAtUtc,
                billingDocument.CreatedByUserId,
                billingDocument.LastUpdatedAtUtc,
                billingDocument.LastUpdatedByUserId,
                billingDocument.IssuedAtUtc,
                billingDocument.IssuedByUserId);
        }
    }
}
