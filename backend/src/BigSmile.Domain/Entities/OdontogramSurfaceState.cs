using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class OdontogramSurfaceState : Entity<Guid>
    {
        private static readonly string[] AllowedSurfaceCodesOrdered = ["O", "M", "D", "B", "L"];
        private static readonly HashSet<string> AllowedSurfaceCodes = new(AllowedSurfaceCodesOrdered, StringComparer.Ordinal);

        public Guid OdontogramId { get; private set; }
        public Odontogram Odontogram { get; private set; } = null!;

        public string ToothCode { get; private set; } = string.Empty;
        public string SurfaceCode { get; private set; } = string.Empty;
        public OdontogramSurfaceStatus Status { get; private set; } = OdontogramSurfaceStatus.Unknown;
        public DateTime UpdatedAtUtc { get; private set; }
        public Guid UpdatedByUserId { get; private set; }

        private OdontogramSurfaceState()
        {
        }

        internal OdontogramSurfaceState(
            Guid odontogramId,
            string toothCode,
            string surfaceCode,
            OdontogramSurfaceStatus status,
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
            ToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            SurfaceCode = NormalizeSurfaceCode(surfaceCode);
            Status = status;
            UpdatedAtUtc = updatedAtUtc;
            UpdatedByUserId = updatedByUserId;
        }

        internal bool UpdateStatus(OdontogramSurfaceStatus status, Guid updatedByUserId)
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

        public static string NormalizeSurfaceCode(string surfaceCode)
        {
            if (string.IsNullOrWhiteSpace(surfaceCode))
            {
                throw new ArgumentException("Surface code is required.", nameof(surfaceCode));
            }

            var normalized = surfaceCode.Trim().ToUpperInvariant();
            if (!AllowedSurfaceCodes.Contains(normalized))
            {
                throw new ArgumentException(
                    "Surface code must be one of: O, M, D, B, or L.",
                    nameof(surfaceCode));
            }

            return normalized;
        }

        public static IReadOnlyList<string> GetAllowedSurfaceCodes()
        {
            return AllowedSurfaceCodesOrdered;
        }

        public static int GetSurfaceSortOrder(string surfaceCode)
        {
            var normalized = NormalizeSurfaceCode(surfaceCode);
            return Array.IndexOf(AllowedSurfaceCodesOrdered, normalized);
        }

        private static void EnsureActor(Guid updatedByUserId)
        {
            if (updatedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram surface actor is required.", nameof(updatedByUserId));
            }
        }
    }
}
