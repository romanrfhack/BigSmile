using BigSmile.Application.Features.Patients.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Patients.Queries
{
    public interface IPatientQueryService
    {
        Task<PatientDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(
            string? searchTerm,
            bool includeInactive,
            int take,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientQueryService : IPatientQueryService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public PatientQueryService(IPatientRepository patientRepository, ITenantContext tenantContext)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<PatientDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();
            var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);
            return patient?.ToDetailDto();
        }

        public async Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(
            string? searchTerm,
            bool includeInactive,
            int take,
            CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();
            var normalizedTake = Math.Clamp(take, 1, 100);
            var patients = await _patientRepository.SearchAsync(searchTerm, includeInactive, normalizedTake, cancellationToken);

            return patients
                .Select(patient => patient.ToSummaryDto())
                .ToList();
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient queries require a resolved tenant context.");
            }
        }
    }
}
