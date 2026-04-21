using BigSmile.Application.Features.TreatmentQuotes.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.TreatmentQuotes.Queries
{
    public interface ITreatmentQuoteQueryService
    {
        Task<TreatmentQuoteDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    }

    public sealed class TreatmentQuoteQueryService : ITreatmentQuoteQueryService
    {
        private readonly ITreatmentQuoteRepository _treatmentQuoteRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public TreatmentQuoteQueryService(
            ITreatmentQuoteRepository treatmentQuoteRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _treatmentQuoteRepository = treatmentQuoteRepository ?? throw new ArgumentNullException(nameof(treatmentQuoteRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<TreatmentQuoteDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var treatmentQuote = await _treatmentQuoteRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return treatmentQuote?.ToDetailDto();
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Treatment quote queries require a resolved tenant context.");
            }
        }
    }
}
