using BigSmile.Application.Features.BillingDocuments.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.BillingDocuments.Queries
{
    public interface IBillingDocumentQueryService
    {
        Task<BillingDocumentDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    }

    public sealed class BillingDocumentQueryService : IBillingDocumentQueryService
    {
        private readonly IBillingDocumentRepository _billingDocumentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public BillingDocumentQueryService(
            IBillingDocumentRepository billingDocumentRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _billingDocumentRepository = billingDocumentRepository ?? throw new ArgumentNullException(nameof(billingDocumentRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<BillingDocumentDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var billingDocument = await _billingDocumentRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return billingDocument?.ToDetailDto();
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Billing document queries require a resolved tenant context.");
            }
        }
    }
}
