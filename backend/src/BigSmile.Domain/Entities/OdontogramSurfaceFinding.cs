using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public sealed class OdontogramSurfaceFinding : Entity<Guid>
    {
        public Guid OdontogramId { get; private set; }
        public Odontogram Odontogram { get; private set; } = null!;

        public string ToothCode { get; private set; } = string.Empty;
        public string SurfaceCode { get; private set; } = string.Empty;
        public OdontogramSurfaceFindingType FindingType { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private OdontogramSurfaceFinding()
        {
        }

        internal OdontogramSurfaceFinding(
            Guid odontogramId,
            string toothCode,
            string surfaceCode,
            OdontogramSurfaceFindingType findingType,
            Guid createdByUserId,
            DateTime createdAtUtc)
        {
            if (odontogramId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram reference is required.", nameof(odontogramId));
            }

            EnsureActor(createdByUserId);

            Id = Guid.NewGuid();
            OdontogramId = odontogramId;
            ToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            SurfaceCode = OdontogramSurfaceState.NormalizeSurfaceCode(surfaceCode);
            FindingType = findingType;
            CreatedAtUtc = createdAtUtc;
            CreatedByUserId = createdByUserId;
        }

        private static void EnsureActor(Guid createdByUserId)
        {
            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram surface finding author is required.", nameof(createdByUserId));
            }
        }
    }
}
