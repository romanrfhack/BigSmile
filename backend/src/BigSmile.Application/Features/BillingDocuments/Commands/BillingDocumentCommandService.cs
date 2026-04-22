using BigSmile.Application.Features.BillingDocuments.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.BillingDocuments.Commands
{
    public sealed record ChangeBillingDocumentStatusCommand(
        string Status);

    public interface IBillingDocumentCommandService
    {
        Task<BillingDocumentDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task<BillingDocumentDetailDto?> ChangeStatusAsync(
            Guid patientId,
            ChangeBillingDocumentStatusCommand command,
            CancellationToken cancellationToken = default);
    }

    public sealed class BillingDocumentCommandService : IBillingDocumentCommandService
    {
        private readonly IBillingDocumentRepository _billingDocumentRepository;
        private readonly ITreatmentQuoteRepository _treatmentQuoteRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public BillingDocumentCommandService(
            IBillingDocumentRepository billingDocumentRepository,
            ITreatmentQuoteRepository treatmentQuoteRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _billingDocumentRepository = billingDocumentRepository ?? throw new ArgumentNullException(nameof(billingDocumentRepository));
            _treatmentQuoteRepository = treatmentQuoteRepository ?? throw new ArgumentNullException(nameof(treatmentQuoteRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<BillingDocumentDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentQuote = await _treatmentQuoteRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentQuote is null)
            {
                throw new InvalidOperationException("An accepted treatment quote is required before creating a billing document.");
            }

            if (treatmentQuote.Status != TreatmentQuoteStatus.Accepted)
            {
                throw new InvalidOperationException("Billing document creation requires the existing treatment quote to be Accepted.");
            }

            if (treatmentQuote.Items.Count == 0)
            {
                throw new InvalidOperationException("Billing document creation requires the accepted treatment quote to contain at least one item.");
            }

            var existingBillingDocument = await _billingDocumentRepository.GetByTreatmentQuoteIdAsync(treatmentQuote.Id, cancellationToken);
            if (existingBillingDocument is not null)
            {
                throw new InvalidOperationException("A billing document already exists for the requested treatment quote.");
            }

            var billingDocument = new BillingDocument(
                tenantId,
                patient.Id,
                treatmentQuote.Id,
                treatmentQuote.CurrencyCode,
                treatmentQuote.Items,
                actorUserId);

            await _billingDocumentRepository.AddAsync(billingDocument, cancellationToken);
            return billingDocument.ToDetailDto();
        }

        public async Task<BillingDocumentDetailDto?> ChangeStatusAsync(
            Guid patientId,
            ChangeBillingDocumentStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var billingDocument = await _billingDocumentRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (billingDocument is null)
            {
                return null;
            }

            var newStatus = ParseStatus(command.Status);
            var changed = billingDocument.ChangeStatus(newStatus, actorUserId);
            if (changed)
            {
                await _billingDocumentRepository.UpdateAsync(billingDocument, cancellationToken);
            }

            return billingDocument.ToDetailDto();
        }

        private async Task<Patient> GetRequiredPatientAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                throw new InvalidOperationException("The requested patient is not available in the current tenant scope.");
            }

            return patient;
        }

        private static void EnsurePatientBelongsToTenant(Patient patient, Guid tenantId)
        {
            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException("Billing documents can only reference patients from the current tenant.");
            }
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Billing document operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Billing document write operations require a resolved user context.");
            }

            return userId;
        }

        private static BillingDocumentStatus ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Billing document status is required.", nameof(status));
            }

            if (!Enum.TryParse<BillingDocumentStatus>(status.Trim(), ignoreCase: true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus))
            {
                throw new ArgumentException(
                    "Billing document status must be one of: Draft or Issued.",
                    nameof(status));
            }

            return parsedStatus;
        }
    }
}
