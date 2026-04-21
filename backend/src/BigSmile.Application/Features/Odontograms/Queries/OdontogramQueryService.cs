using BigSmile.Application.Features.Odontograms.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Odontograms.Queries
{
    public interface IOdontogramQueryService
    {
        Task<OdontogramDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    }

    public sealed class OdontogramQueryService : IOdontogramQueryService
    {
        private readonly IOdontogramRepository _odontogramRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public OdontogramQueryService(
            IOdontogramRepository odontogramRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _odontogramRepository = odontogramRepository ?? throw new ArgumentNullException(nameof(odontogramRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<OdontogramDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var odontogram = await _odontogramRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return odontogram?.ToDetailDto();
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Odontogram queries require a resolved tenant context.");
            }
        }
    }
}
