using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class TreatmentPlan : Entity<Guid>, ITenantOwnedEntity
    {
        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public TreatmentPlanStatus Status { get; private set; } = TreatmentPlanStatus.Draft;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public Guid LastUpdatedByUserId { get; private set; }

        public ICollection<TreatmentPlanItem> Items { get; private set; } = new List<TreatmentPlanItem>();

        private TreatmentPlan()
        {
        }

        public TreatmentPlan(Guid tenantId, Guid patientId, Guid createdByUserId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Treatment plan tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Treatment plan patient reference is required.", nameof(patientId));
            }

            EnsureActor(createdByUserId);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            Status = TreatmentPlanStatus.Draft;
            CreatedAtUtc = DateTime.UtcNow;
            CreatedByUserId = createdByUserId;
            LastUpdatedAtUtc = CreatedAtUtc;
            LastUpdatedByUserId = createdByUserId;
        }

        public TreatmentPlanItem AddItem(
            string title,
            string? category,
            int quantity,
            string? notes,
            string? toothCode,
            string? surfaceCode,
            Guid createdByUserId)
        {
            EnsureEditable();
            EnsureActor(createdByUserId);

            var item = new TreatmentPlanItem(
                Id,
                title,
                category,
                quantity,
                notes,
                toothCode,
                surfaceCode,
                createdByUserId,
                DateTime.UtcNow);

            Items.Add(item);
            Touch(createdByUserId);
            return item;
        }

        public void RemoveItem(Guid itemId, Guid removedByUserId)
        {
            EnsureEditable();
            EnsureActor(removedByUserId);

            if (itemId == Guid.Empty)
            {
                throw new ArgumentException("Treatment plan item reference is required.", nameof(itemId));
            }

            var item = Items.SingleOrDefault(entry => entry.Id == itemId);
            if (item is null)
            {
                throw new InvalidOperationException("The requested treatment plan item does not exist in the current plan.");
            }

            Items.Remove(item);
            Touch(removedByUserId);
        }

        public bool ChangeStatus(TreatmentPlanStatus newStatus, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            if (Status == newStatus)
            {
                return false;
            }

            if (!IsAllowedTransition(Status, newStatus))
            {
                throw new InvalidOperationException(
                    "Treatment plan status transitions in this slice are limited to Draft -> Proposed and Proposed -> Draft/Accepted.");
            }

            Status = newStatus;
            Touch(updatedByUserId);
            return true;
        }

        private void EnsureEditable()
        {
            if (Status == TreatmentPlanStatus.Accepted)
            {
                throw new InvalidOperationException("Accepted treatment plans are read-only in this slice.");
            }
        }

        private void Touch(Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);
            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
        }

        private static bool IsAllowedTransition(TreatmentPlanStatus currentStatus, TreatmentPlanStatus newStatus)
        {
            return currentStatus switch
            {
                TreatmentPlanStatus.Draft => newStatus == TreatmentPlanStatus.Proposed,
                TreatmentPlanStatus.Proposed => newStatus is TreatmentPlanStatus.Draft or TreatmentPlanStatus.Accepted,
                TreatmentPlanStatus.Accepted => false,
                _ => false
            };
        }

        private static void EnsureActor(Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Treatment plan actor is required.", nameof(actorUserId));
            }
        }
    }
}
