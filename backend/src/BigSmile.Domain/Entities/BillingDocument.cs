using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class BillingDocument : Entity<Guid>, ITenantOwnedEntity
    {
        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public Guid TreatmentQuoteId { get; private set; }
        public TreatmentQuote TreatmentQuote { get; private set; } = null!;

        public BillingDocumentStatus Status { get; private set; } = BillingDocumentStatus.Draft;
        public string CurrencyCode { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public Guid LastUpdatedByUserId { get; private set; }
        public DateTime? IssuedAtUtc { get; private set; }
        public Guid? IssuedByUserId { get; private set; }

        public ICollection<BillingDocumentItem> Items { get; private set; } = new List<BillingDocumentItem>();

        private BillingDocument()
        {
        }

        public BillingDocument(
            Guid tenantId,
            Guid patientId,
            Guid treatmentQuoteId,
            string currencyCode,
            IEnumerable<TreatmentQuoteItem> sourceItems,
            Guid createdByUserId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Billing document tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Billing document patient reference is required.", nameof(patientId));
            }

            if (treatmentQuoteId == Guid.Empty)
            {
                throw new ArgumentException("Billing document treatment quote reference is required.", nameof(treatmentQuoteId));
            }

            EnsureActor(createdByUserId);

            var snapshotItems = sourceItems?.ToList()
                ?? throw new ArgumentNullException(nameof(sourceItems));

            if (snapshotItems.Count == 0)
            {
                throw new ArgumentException("Billing document creation requires at least one treatment quote item.", nameof(sourceItems));
            }

            if (snapshotItems.Any(item => item is null))
            {
                throw new ArgumentException("Billing document source items cannot contain null entries.", nameof(sourceItems));
            }

            if (snapshotItems.Select(item => item.Id).Distinct().Count() != snapshotItems.Count)
            {
                throw new ArgumentException("Billing document source items cannot repeat the same treatment quote item.", nameof(sourceItems));
            }

            if (snapshotItems.Any(item => item.UnitPrice <= 0))
            {
                throw new ArgumentException("Billing document creation requires source quote items with positive unit prices.", nameof(sourceItems));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            TreatmentQuoteId = treatmentQuoteId;
            Status = BillingDocumentStatus.Draft;
            CurrencyCode = NormalizeCurrencyCode(currencyCode);
            CreatedAtUtc = DateTime.UtcNow;
            CreatedByUserId = createdByUserId;
            LastUpdatedAtUtc = CreatedAtUtc;
            LastUpdatedByUserId = createdByUserId;

            foreach (var sourceItem in snapshotItems)
            {
                Items.Add(new BillingDocumentItem(
                    Id,
                    sourceItem.Id,
                    sourceItem.Title,
                    sourceItem.Category,
                    sourceItem.Quantity,
                    sourceItem.Notes,
                    sourceItem.ToothCode,
                    sourceItem.SurfaceCode,
                    sourceItem.UnitPrice,
                    createdByUserId,
                    CreatedAtUtc));
            }

            TotalAmount = Items.Sum(item => item.LineTotal);
        }

        public bool ChangeStatus(BillingDocumentStatus newStatus, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            if (Status == BillingDocumentStatus.Issued)
            {
                throw new InvalidOperationException("Issued billing documents are read-only in this slice.");
            }

            if (Status == newStatus)
            {
                return false;
            }

            if (Status != BillingDocumentStatus.Draft || newStatus != BillingDocumentStatus.Issued)
            {
                throw new InvalidOperationException("Billing document status transitions in this slice are limited to Draft -> Issued.");
            }

            var issuedAtUtc = DateTime.UtcNow;
            Status = BillingDocumentStatus.Issued;
            IssuedAtUtc = issuedAtUtc;
            IssuedByUserId = updatedByUserId;
            LastUpdatedAtUtc = issuedAtUtc;
            LastUpdatedByUserId = updatedByUserId;
            return true;
        }

        private static string NormalizeCurrencyCode(string? currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                throw new ArgumentException("Billing document currency code is required.", nameof(currencyCode));
            }

            var normalized = currencyCode.Trim().ToUpperInvariant();
            if (normalized.Length != 3)
            {
                throw new ArgumentException("Billing document currency code must contain exactly three characters.", nameof(currencyCode));
            }

            return normalized;
        }

        private static void EnsureActor(Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Billing document actor is required.", nameof(actorUserId));
            }
        }
    }
}
