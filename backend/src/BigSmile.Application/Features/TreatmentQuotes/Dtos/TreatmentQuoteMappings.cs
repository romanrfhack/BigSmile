using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.TreatmentQuotes.Dtos
{
    internal static class TreatmentQuoteMappings
    {
        public static TreatmentQuoteDetailDto ToDetailDto(this TreatmentQuote treatmentQuote)
        {
            return new TreatmentQuoteDetailDto(
                treatmentQuote.Id,
                treatmentQuote.PatientId,
                treatmentQuote.TreatmentPlanId,
                treatmentQuote.Status.ToString(),
                treatmentQuote.CurrencyCode,
                treatmentQuote.GetTotal(),
                treatmentQuote.Items
                    .OrderBy(item => item.CreatedAtUtc)
                    .ThenBy(item => item.Id)
                    .Select(item => new TreatmentQuoteItemDto(
                        item.Id,
                        item.SourceTreatmentPlanItemId,
                        item.Title,
                        item.Category,
                        item.Quantity,
                        item.Notes,
                        item.ToothCode,
                        item.SurfaceCode,
                        item.UnitPrice,
                        item.GetLineTotal(),
                        item.CreatedAtUtc,
                        item.CreatedByUserId))
                    .ToList(),
                treatmentQuote.CreatedAtUtc,
                treatmentQuote.CreatedByUserId,
                treatmentQuote.LastUpdatedAtUtc,
                treatmentQuote.LastUpdatedByUserId);
        }
    }
}
