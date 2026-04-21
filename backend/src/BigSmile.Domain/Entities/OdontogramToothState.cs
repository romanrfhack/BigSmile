using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class OdontogramToothState : Entity<Guid>
    {
        private static readonly HashSet<string> AllowedToothCodes = new(
            new[]
            {
                "11", "12", "13", "14", "15", "16", "17", "18",
                "21", "22", "23", "24", "25", "26", "27", "28",
                "31", "32", "33", "34", "35", "36", "37", "38",
                "41", "42", "43", "44", "45", "46", "47", "48"
            },
            StringComparer.Ordinal);

        public Guid OdontogramId { get; private set; }
        public Odontogram Odontogram { get; private set; } = null!;

        public string ToothCode { get; private set; } = string.Empty;
        public OdontogramToothStatus Status { get; private set; } = OdontogramToothStatus.Unknown;
        public DateTime UpdatedAtUtc { get; private set; }
        public Guid UpdatedByUserId { get; private set; }

        private OdontogramToothState()
        {
        }

        internal OdontogramToothState(
            Guid odontogramId,
            string toothCode,
            OdontogramToothStatus status,
            Guid updatedByUserId,
            DateTime updatedAtUtc)
        {
            if (odontogramId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram reference is required.", nameof(odontogramId));
            }

            EnsureActor(updatedByUserId);

            Id = Guid.NewGuid();
            OdontogramId = odontogramId;
            ToothCode = NormalizeToothCode(toothCode);
            Status = status;
            UpdatedAtUtc = updatedAtUtc;
            UpdatedByUserId = updatedByUserId;
        }

        internal bool UpdateStatus(OdontogramToothStatus status, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            if (Status == status)
            {
                return false;
            }

            Status = status;
            UpdatedAtUtc = DateTime.UtcNow;
            UpdatedByUserId = updatedByUserId;
            return true;
        }

        public static string NormalizeToothCode(string toothCode)
        {
            if (string.IsNullOrWhiteSpace(toothCode))
            {
                throw new ArgumentException("Tooth code is required.", nameof(toothCode));
            }

            var normalized = toothCode.Trim();
            if (!AllowedToothCodes.Contains(normalized))
            {
                throw new ArgumentException(
                    "Tooth code must use FDI permanent adult numbering and be one of: 11-18, 21-28, 31-38, or 41-48.",
                    nameof(toothCode));
            }

            return normalized;
        }

        public static IReadOnlyCollection<string> GetAllowedToothCodes()
        {
            return AllowedToothCodes
                .OrderBy(code => code, StringComparer.Ordinal)
                .ToArray();
        }

        private static void EnsureActor(Guid updatedByUserId)
        {
            if (updatedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram tooth actor is required.", nameof(updatedByUserId));
            }
        }
    }
}
