using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class Odontogram : Entity<Guid>, ITenantOwnedEntity
    {
        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public Guid LastUpdatedByUserId { get; private set; }

        public ICollection<OdontogramToothState> Teeth { get; private set; } = new List<OdontogramToothState>();
        public ICollection<OdontogramSurfaceState> Surfaces { get; private set; } = new List<OdontogramSurfaceState>();

        private Odontogram()
        {
        }

        public Odontogram(Guid tenantId, Guid patientId, Guid createdByUserId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram patient reference is required.", nameof(patientId));
            }

            EnsureActor(createdByUserId);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            CreatedAtUtc = DateTime.UtcNow;
            CreatedByUserId = createdByUserId;
            LastUpdatedAtUtc = CreatedAtUtc;
            LastUpdatedByUserId = createdByUserId;

            foreach (var toothCode in OdontogramToothState.GetAllowedToothCodes())
            {
                Teeth.Add(new OdontogramToothState(
                    Id,
                    toothCode,
                    OdontogramToothStatus.Unknown,
                    createdByUserId,
                    CreatedAtUtc));

                foreach (var surfaceCode in OdontogramSurfaceState.GetAllowedSurfaceCodes())
                {
                    Surfaces.Add(new OdontogramSurfaceState(
                        Id,
                        toothCode,
                        surfaceCode,
                        OdontogramSurfaceStatus.Unknown,
                        createdByUserId,
                        CreatedAtUtc));
                }
            }
        }

        public bool UpdateToothStatus(string toothCode, OdontogramToothStatus status, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            var normalizedToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            var toothState = Teeth.SingleOrDefault(tooth => tooth.ToothCode == normalizedToothCode);
            if (toothState is null)
            {
                throw new InvalidOperationException("The requested tooth does not exist in the current odontogram.");
            }

            var changed = toothState.UpdateStatus(status, updatedByUserId);
            if (!changed)
            {
                return false;
            }

            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
            return true;
        }

        public bool UpdateSurfaceStatus(string toothCode, string surfaceCode, OdontogramSurfaceStatus status, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            var normalizedToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            var normalizedSurfaceCode = OdontogramSurfaceState.NormalizeSurfaceCode(surfaceCode);
            var surfaceState = Surfaces.SingleOrDefault(surface =>
                surface.ToothCode == normalizedToothCode &&
                surface.SurfaceCode == normalizedSurfaceCode);
            if (surfaceState is null)
            {
                throw new InvalidOperationException("The requested tooth surface does not exist in the current odontogram.");
            }

            var changed = surfaceState.UpdateStatus(status, updatedByUserId);
            if (!changed)
            {
                return false;
            }

            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
            return true;
        }

        private static void EnsureActor(Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram actor is required.", nameof(actorUserId));
            }
        }
    }
}
