using BigSmile.Application.Features.TreatmentPlans.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.TreatmentPlans.Queries
{
    public interface ITreatmentPlanQueryService
    {
        Task<TreatmentPlanDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    }

    public sealed class TreatmentPlanQueryService : ITreatmentPlanQueryService
    {
        private readonly ITreatmentPlanRepository _treatmentPlanRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public TreatmentPlanQueryService(
            ITreatmentPlanRepository treatmentPlanRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _treatmentPlanRepository = treatmentPlanRepository ?? throw new ArgumentNullException(nameof(treatmentPlanRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<TreatmentPlanDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var treatmentPlan = await _treatmentPlanRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return treatmentPlan?.ToDetailDto();
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Treatment plan queries require a resolved tenant context.");
            }
        }
    }
}
