using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class BillingDocumentItem : Entity<Guid>
    {
        private const int TitleMaxLength = 200;
        private const int CategoryMaxLength = 100;
        private const int NotesMaxLength = 500;

        public Guid BillingDocumentId { get; private set; }
        public BillingDocument BillingDocument { get; private set; } = null!;

        public Guid SourceTreatmentQuoteItemId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Category { get; private set; }
        public int Quantity { get; private set; }
        public string? Notes { get; private set; }
        public string? ToothCode { get; private set; }
        public string? SurfaceCode { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal LineTotal { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private BillingDocumentItem()
        {
        }

        internal BillingDocumentItem(
            Guid billingDocumentId,
            Guid sourceTreatmentQuoteItemId,
            string title,
            string? category,
            int quantity,
            string? notes,
            string? toothCode,
            string? surfaceCode,
            decimal unitPrice,
            Guid createdByUserId,
            DateTime createdAtUtc)
        {
            if (billingDocumentId == Guid.Empty)
            {
                throw new ArgumentException("Billing document reference is required.", nameof(billingDocumentId));
            }

            if (sourceTreatmentQuoteItemId == Guid.Empty)
            {
                throw new ArgumentException("Source treatment quote item reference is required.", nameof(sourceTreatmentQuoteItemId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Billing document item quantity must be greater than zero.", nameof(quantity));
            }

            EnsureActor(createdByUserId);

            Id = Guid.NewGuid();
            BillingDocumentId = billingDocumentId;
            SourceTreatmentQuoteItemId = sourceTreatmentQuoteItemId;
            Title = NormalizeRequired(title, nameof(title), TitleMaxLength);
            Category = NormalizeOptional(category, nameof(category), CategoryMaxLength);
            Quantity = quantity;
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            (ToothCode, SurfaceCode) = NormalizeDentalLocation(toothCode, surfaceCode);
            UnitPrice = NormalizeUnitPrice(unitPrice);
            LineTotal = Quantity * UnitPrice;
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = createdAtUtc;
        }

        private static decimal NormalizeUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
            {
                throw new ArgumentException("Billing document item unit price must be greater than or equal to zero.", nameof(unitPrice));
            }

            return unitPrice;
        }

        private static (string? ToothCode, string? SurfaceCode) NormalizeDentalLocation(string? toothCode, string? surfaceCode)
        {
            var normalizedToothCode = NormalizeOptional(toothCode, nameof(toothCode), 2);
            var normalizedSurfaceCode = NormalizeOptional(surfaceCode, nameof(surfaceCode), 1);

            if (normalizedSurfaceCode is not null && normalizedToothCode is null)
            {
                throw new ArgumentException("SurfaceCode requires ToothCode in billing document items.", nameof(surfaceCode));
            }

            if (normalizedToothCode is null)
            {
                return (null, null);
            }

            var validatedToothCode = OdontogramToothState.NormalizeToothCode(normalizedToothCode);
            var validatedSurfaceCode = normalizedSurfaceCode is null
                ? null
                : OdontogramSurfaceState.NormalizeSurfaceCode(normalizedSurfaceCode);

            return (validatedToothCode, validatedSurfaceCode);
        }

        private static string NormalizeRequired(string? value, string parameterName, int maxLength)
        {
            var normalized = NormalizeOptional(value, parameterName, maxLength);
            if (normalized is null)
            {
                throw new ArgumentException($"{parameterName} is required.", parameterName);
            }

            return normalized;
        }

        private static string? NormalizeOptional(string? value, string parameterName, int maxLength)
        {
            if (value is null)
            {
                return null;
            }

            var normalized = value.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return null;
            }

            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{parameterName} cannot exceed {maxLength} characters.", parameterName);
            }

            return normalized;
        }

        private static void EnsureActor(Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Billing document item author is required.", nameof(actorUserId));
            }
        }
    }
}
