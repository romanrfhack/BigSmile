using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.ClinicalRecords.Queries
{
    public interface IClinicalRecordQueryService
    {
        Task<ClinicalRecordDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<ClinicalMedicalQuestionnaireDto?> GetQuestionnaireByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ClinicalEncounterDto>?> GetEncountersByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    }

    public sealed class ClinicalRecordQueryService : IClinicalRecordQueryService
    {
        private readonly IClinicalRecordRepository _clinicalRecordRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public ClinicalRecordQueryService(
            IClinicalRecordRepository clinicalRecordRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _clinicalRecordRepository = clinicalRecordRepository ?? throw new ArgumentNullException(nameof(clinicalRecordRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<ClinicalRecordDetailDto?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return clinicalRecord?.ToDetailDto();
        }

        public async Task<ClinicalMedicalQuestionnaireDto?> GetQuestionnaireByPatientIdAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return clinicalRecord?.ToQuestionnaireDto();
        }

        public async Task<IReadOnlyList<ClinicalEncounterDto>?> GetEncountersByPatientIdAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            return clinicalRecord?.ToEncounterDtos();
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Clinical record queries require a resolved tenant context.");
            }
        }
    }
}
