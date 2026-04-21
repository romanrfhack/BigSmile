using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class TreatmentQuoteItem : Entity<Guid>
    {
        private const int TitleMaxLength = 200;
        private const int CategoryMaxLength = 100;
        private const int NotesMaxLength = 500;

        public Guid TreatmentQuoteId { get; private set; }
        public TreatmentQuote TreatmentQuote { get; private set; } = null!;

        public Guid SourceTreatmentPlanItemId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string? Category { get; private set; }
        public int Quantity { get; private set; }
        public string? Notes { get; private set; }
        public string? ToothCode { get; private set; }
        public string? SurfaceCode { get; private set; }
        public decimal UnitPrice { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private TreatmentQuoteItem()
        {
        }

        internal TreatmentQuoteItem(
            Guid treatmentQuoteId,
            Guid sourceTreatmentPlanItemId,
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
            if (treatmentQuoteId == Guid.Empty)
            {
                throw new ArgumentException("Treatment quote reference is required.", nameof(treatmentQuoteId));
            }

            if (sourceTreatmentPlanItemId == Guid.Empty)
            {
                throw new ArgumentException("Source treatment plan item reference is required.", nameof(sourceTreatmentPlanItemId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Treatment quote item quantity must be greater than zero.", nameof(quantity));
            }

            EnsureActor(createdByUserId);

            Id = Guid.NewGuid();
            TreatmentQuoteId = treatmentQuoteId;
            SourceTreatmentPlanItemId = sourceTreatmentPlanItemId;
            Title = NormalizeRequired(title, nameof(title), TitleMaxLength);
            Category = NormalizeOptional(category, nameof(category), CategoryMaxLength);
            Quantity = quantity;
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            (ToothCode, SurfaceCode) = NormalizeDentalLocation(toothCode, surfaceCode);
            UnitPrice = NormalizeUnitPrice(unitPrice);
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = createdAtUtc;
        }

        public decimal GetLineTotal()
        {
            return Quantity * UnitPrice;
        }

        internal bool UpdateUnitPrice(decimal unitPrice)
        {
            var normalizedUnitPrice = NormalizeUnitPrice(unitPrice);
            if (UnitPrice == normalizedUnitPrice)
            {
                return false;
            }

            UnitPrice = normalizedUnitPrice;
            return true;
        }

        private static decimal NormalizeUnitPrice(decimal unitPrice)
        {
            if (unitPrice < 0)
            {
                throw new ArgumentException("Treatment quote item unit price must be greater than or equal to zero.", nameof(unitPrice));
            }

            return unitPrice;
        }

        private static (string? ToothCode, string? SurfaceCode) NormalizeDentalLocation(string? toothCode, string? surfaceCode)
        {
            var normalizedToothCode = NormalizeOptional(toothCode, nameof(toothCode), 2);
            var normalizedSurfaceCode = NormalizeOptional(surfaceCode, nameof(surfaceCode), 1);

            if (normalizedSurfaceCode is not null && normalizedToothCode is null)
            {
                throw new ArgumentException("SurfaceCode requires ToothCode in treatment quote items.", nameof(surfaceCode));
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
                throw new ArgumentException("Treatment quote item author is required.", nameof(actorUserId));
            }
        }
    }
}
