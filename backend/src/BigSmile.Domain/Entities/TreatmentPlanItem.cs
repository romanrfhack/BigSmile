using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class TreatmentPlanItem : Entity<Guid>
    {
        private const int TitleMaxLength = 200;
        private const int CategoryMaxLength = 100;
        private const int NotesMaxLength = 500;

        public Guid TreatmentPlanId { get; private set; }
        public TreatmentPlan TreatmentPlan { get; private set; } = null!;

        public string Title { get; private set; } = string.Empty;
        public string? Category { get; private set; }
        public int Quantity { get; private set; }
        public string? Notes { get; private set; }
        public string? ToothCode { get; private set; }
        public string? SurfaceCode { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private TreatmentPlanItem()
        {
        }

        internal TreatmentPlanItem(
            Guid treatmentPlanId,
            string title,
            string? category,
            int quantity,
            string? notes,
            string? toothCode,
            string? surfaceCode,
            Guid createdByUserId,
            DateTime createdAtUtc)
        {
            if (treatmentPlanId == Guid.Empty)
            {
                throw new ArgumentException("Treatment plan reference is required.", nameof(treatmentPlanId));
            }

            if (quantity <= 0)
            {
                throw new ArgumentException("Treatment plan item quantity must be greater than zero.", nameof(quantity));
            }

            EnsureActor(createdByUserId);

            Id = Guid.NewGuid();
            TreatmentPlanId = treatmentPlanId;
            Title = NormalizeRequired(title, nameof(title), TitleMaxLength);
            Category = NormalizeOptional(category, nameof(category), CategoryMaxLength);
            Quantity = quantity;
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            (ToothCode, SurfaceCode) = NormalizeDentalLocation(toothCode, surfaceCode);
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = createdAtUtc;
        }

        private static (string? ToothCode, string? SurfaceCode) NormalizeDentalLocation(string? toothCode, string? surfaceCode)
        {
            var normalizedToothCode = NormalizeOptional(toothCode, nameof(toothCode), 2);
            var normalizedSurfaceCode = NormalizeOptional(surfaceCode, nameof(surfaceCode), 1);

            if (normalizedSurfaceCode is not null && normalizedToothCode is null)
            {
                throw new ArgumentException("SurfaceCode requires ToothCode in treatment plan items.", nameof(surfaceCode));
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
                throw new ArgumentException("Treatment plan item author is required.", nameof(actorUserId));
            }
        }
    }
}
