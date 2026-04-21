using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class OdontogramSurfaceFindingHistoryEntry : Entity<Guid>
    {
        public const int SummaryMaxLength = 200;

        public Guid OdontogramId { get; private set; }
        public Odontogram Odontogram { get; private set; } = null!;

        public string ToothCode { get; private set; } = string.Empty;
        public string SurfaceCode { get; private set; } = string.Empty;
        public OdontogramSurfaceFindingType FindingType { get; private set; }
        public OdontogramSurfaceFindingHistoryEntryType EntryType { get; private set; }
        public string Summary { get; private set; } = string.Empty;
        public DateTime ChangedAtUtc { get; private set; }
        public Guid ChangedByUserId { get; private set; }
        public Guid? ReferenceFindingId { get; private set; }

        private OdontogramSurfaceFindingHistoryEntry()
        {
        }

        internal OdontogramSurfaceFindingHistoryEntry(
            Guid odontogramId,
            string toothCode,
            string surfaceCode,
            OdontogramSurfaceFindingType findingType,
            OdontogramSurfaceFindingHistoryEntryType entryType,
            string summary,
            Guid changedByUserId,
            Guid? referenceFindingId)
        {
            if (odontogramId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram reference is required.", nameof(odontogramId));
            }

            if (changedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram surface finding history actor is required.", nameof(changedByUserId));
            }

            if (referenceFindingId.HasValue && referenceFindingId.Value == Guid.Empty)
            {
                throw new ArgumentException("Reference finding id cannot be empty when provided.", nameof(referenceFindingId));
            }

            Id = Guid.NewGuid();
            OdontogramId = odontogramId;
            ToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            SurfaceCode = OdontogramSurfaceState.NormalizeSurfaceCode(surfaceCode);
            FindingType = findingType;
            EntryType = entryType;
            Summary = NormalizeRequired(summary, nameof(summary), SummaryMaxLength);
            ChangedAtUtc = DateTime.UtcNow;
            ChangedByUserId = changedByUserId;
            ReferenceFindingId = referenceFindingId;
        }

        private static string NormalizeRequired(string value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{paramName} is required.", paramName);
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} exceeds the allowed length of {maxLength}.", paramName);
            }

            return normalized;
        }
    }
}
