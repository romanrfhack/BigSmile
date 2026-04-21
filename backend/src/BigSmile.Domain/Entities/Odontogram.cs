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
        public ICollection<OdontogramSurfaceFinding> SurfaceFindings { get; private set; } = new List<OdontogramSurfaceFinding>();

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
            var surfaceState = GetRequiredSurfaceState(normalizedToothCode, normalizedSurfaceCode);

            var changed = surfaceState.UpdateStatus(status, updatedByUserId);
            if (!changed)
            {
                return false;
            }

            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
            return true;
        }

        public OdontogramSurfaceFinding AddSurfaceFinding(
            string toothCode,
            string surfaceCode,
            OdontogramSurfaceFindingType findingType,
            Guid createdByUserId)
        {
            EnsureActor(createdByUserId);

            var normalizedToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            var normalizedSurfaceCode = OdontogramSurfaceState.NormalizeSurfaceCode(surfaceCode);
            GetRequiredSurfaceState(normalizedToothCode, normalizedSurfaceCode);

            if (SurfaceFindings.Any(finding =>
                    finding.ToothCode == normalizedToothCode &&
                    finding.SurfaceCode == normalizedSurfaceCode &&
                    finding.FindingType == findingType))
            {
                throw new InvalidOperationException("The requested finding already exists on the current tooth surface.");
            }

            var finding = new OdontogramSurfaceFinding(
                Id,
                normalizedToothCode,
                normalizedSurfaceCode,
                findingType,
                createdByUserId,
                DateTime.UtcNow);

            SurfaceFindings.Add(finding);
            Touch(createdByUserId);

            return finding;
        }

        public void RemoveSurfaceFinding(
            string toothCode,
            string surfaceCode,
            Guid findingId,
            Guid removedByUserId)
        {
            EnsureActor(removedByUserId);

            if (findingId == Guid.Empty)
            {
                throw new ArgumentException("Odontogram surface finding reference is required.", nameof(findingId));
            }

            var normalizedToothCode = OdontogramToothState.NormalizeToothCode(toothCode);
            var normalizedSurfaceCode = OdontogramSurfaceState.NormalizeSurfaceCode(surfaceCode);
            GetRequiredSurfaceState(normalizedToothCode, normalizedSurfaceCode);

            var finding = SurfaceFindings.SingleOrDefault(entry =>
                entry.Id == findingId &&
                entry.ToothCode == normalizedToothCode &&
                entry.SurfaceCode == normalizedSurfaceCode);

            if (finding is null)
            {
                throw new InvalidOperationException("The requested finding does not exist on the current tooth surface.");
            }

            SurfaceFindings.Remove(finding);
            Touch(removedByUserId);
        }

        private OdontogramSurfaceState GetRequiredSurfaceState(string toothCode, string surfaceCode)
        {
            var surfaceState = Surfaces.SingleOrDefault(surface =>
                surface.ToothCode == toothCode &&
                surface.SurfaceCode == surfaceCode);

            if (surfaceState is null)
            {
                throw new InvalidOperationException("The requested tooth surface does not exist in the current odontogram.");
            }

            return surfaceState;
        }

        private void Touch(Guid updatedByUserId)
        {
            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
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
