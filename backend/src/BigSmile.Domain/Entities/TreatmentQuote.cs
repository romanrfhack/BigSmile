using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class TreatmentQuote : Entity<Guid>, ITenantOwnedEntity
    {
        public const string DefaultCurrencyCode = "MXN";

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public Guid TreatmentPlanId { get; private set; }
        public TreatmentPlan TreatmentPlan { get; private set; } = null!;

        public TreatmentQuoteStatus Status { get; private set; } = TreatmentQuoteStatus.Draft;
        public string CurrencyCode { get; private set; } = DefaultCurrencyCode;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public Guid LastUpdatedByUserId { get; private set; }

        public ICollection<TreatmentQuoteItem> Items { get; private set; } = new List<TreatmentQuoteItem>();

        private TreatmentQuote()
        {
        }

        public TreatmentQuote(
            Guid tenantId,
            Guid patientId,
            Guid treatmentPlanId,
            IEnumerable<TreatmentPlanItem> sourceItems,
            Guid createdByUserId,
            string? currencyCode = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Treatment quote tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Treatment quote patient reference is required.", nameof(patientId));
            }

            if (treatmentPlanId == Guid.Empty)
            {
                throw new ArgumentException("Treatment quote treatment plan reference is required.", nameof(treatmentPlanId));
            }

            EnsureActor(createdByUserId);

            var snapshotItems = sourceItems?.ToList()
                ?? throw new ArgumentNullException(nameof(sourceItems));

            if (snapshotItems.Count == 0)
            {
                throw new ArgumentException("Treatment quote creation requires at least one treatment plan item.", nameof(sourceItems));
            }

            if (snapshotItems.Any(item => item is null))
            {
                throw new ArgumentException("Treatment quote source items cannot contain null entries.", nameof(sourceItems));
            }

            if (snapshotItems.Select(item => item.Id).Distinct().Count() != snapshotItems.Count)
            {
                throw new ArgumentException("Treatment quote source items cannot repeat the same treatment plan item.", nameof(sourceItems));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            TreatmentPlanId = treatmentPlanId;
            Status = TreatmentQuoteStatus.Draft;
            CurrencyCode = NormalizeCurrencyCode(currencyCode);
            CreatedAtUtc = DateTime.UtcNow;
            CreatedByUserId = createdByUserId;
            LastUpdatedAtUtc = CreatedAtUtc;
            LastUpdatedByUserId = createdByUserId;

            foreach (var sourceItem in snapshotItems)
            {
                Items.Add(new TreatmentQuoteItem(
                    Id,
                    sourceItem.Id,
                    sourceItem.Title,
                    sourceItem.Category,
                    sourceItem.Quantity,
                    sourceItem.Notes,
                    sourceItem.ToothCode,
                    sourceItem.SurfaceCode,
                    unitPrice: 0m,
                    createdByUserId,
                    CreatedAtUtc));
            }
        }

        public decimal GetTotal()
        {
            return Items.Sum(item => item.GetLineTotal());
        }

        public bool UpdateItemUnitPrice(Guid quoteItemId, decimal unitPrice, Guid updatedByUserId)
        {
            EnsureEditable();
            EnsureActor(updatedByUserId);

            if (quoteItemId == Guid.Empty)
            {
                throw new ArgumentException("Treatment quote item reference is required.", nameof(quoteItemId));
            }

            var item = Items.SingleOrDefault(entry => entry.Id == quoteItemId);
            if (item is null)
            {
                throw new InvalidOperationException("The requested treatment quote item does not exist in the current quote.");
            }

            if (Status == TreatmentQuoteStatus.Proposed)
            {
                EnsureProposedPricingInvariant(quoteItemId, unitPrice);
            }

            var changed = item.UpdateUnitPrice(unitPrice);
            if (changed)
            {
                Touch(updatedByUserId);
            }

            return changed;
        }

        public bool ChangeStatus(TreatmentQuoteStatus newStatus, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            if (Status == newStatus)
            {
                return false;
            }

            if (!IsAllowedTransition(Status, newStatus))
            {
                throw new InvalidOperationException(
                    "Treatment quote status transitions in this slice are limited to Draft -> Proposed and Proposed -> Draft/Accepted.");
            }

            if ((Status == TreatmentQuoteStatus.Draft && newStatus == TreatmentQuoteStatus.Proposed) ||
                (Status == TreatmentQuoteStatus.Proposed && newStatus == TreatmentQuoteStatus.Accepted))
            {
                EnsureReadyForCommittedStatus();
            }

            Status = newStatus;
            Touch(updatedByUserId);
            return true;
        }

        private void EnsureReadyForCommittedStatus()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("Treatment quotes require at least one item before moving to Proposed or Accepted.");
            }

            if (Items.Any(item => item.UnitPrice <= 0))
            {
                throw new InvalidOperationException("Every treatment quote item must have a unit price greater than zero before moving to Proposed or Accepted.");
            }
        }

        private void EnsureProposedPricingInvariant(Guid quoteItemId, decimal unitPrice)
        {
            if (unitPrice <= 0 || Items.Any(item => item.Id != quoteItemId && item.UnitPrice <= 0))
            {
                throw new InvalidOperationException("Proposed treatment quotes require every item to keep a unit price greater than zero.");
            }
        }

        private void EnsureEditable()
        {
            if (Status == TreatmentQuoteStatus.Accepted)
            {
                throw new InvalidOperationException("Accepted treatment quotes are read-only in this slice.");
            }
        }

        private void Touch(Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);
            LastUpdatedAtUtc = DateTime.UtcNow;
            LastUpdatedByUserId = updatedByUserId;
        }

        private static bool IsAllowedTransition(TreatmentQuoteStatus currentStatus, TreatmentQuoteStatus newStatus)
        {
            return currentStatus switch
            {
                TreatmentQuoteStatus.Draft => newStatus == TreatmentQuoteStatus.Proposed,
                TreatmentQuoteStatus.Proposed => newStatus is TreatmentQuoteStatus.Draft or TreatmentQuoteStatus.Accepted,
                TreatmentQuoteStatus.Accepted => false,
                _ => false
            };
        }

        private static string NormalizeCurrencyCode(string? currencyCode)
        {
            var normalized = string.IsNullOrWhiteSpace(currencyCode)
                ? DefaultCurrencyCode
                : currencyCode.Trim().ToUpperInvariant();

            if (normalized.Length != 3)
            {
                throw new ArgumentException("Treatment quote currency code must contain exactly three characters.", nameof(currencyCode));
            }

            return normalized;
        }

        private static void EnsureActor(Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Treatment quote actor is required.", nameof(actorUserId));
            }
        }
    }
}
