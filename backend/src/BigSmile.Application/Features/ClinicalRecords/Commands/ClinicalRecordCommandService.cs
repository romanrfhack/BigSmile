using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.ClinicalRecords.Commands
{
    public sealed record ClinicalAllergyInput(
        string Substance,
        string? ReactionSummary,
        string? Notes);

    public sealed record SaveClinicalRecordSnapshotCommand(
        string? MedicalBackgroundSummary,
        string? CurrentMedicationsSummary,
        IReadOnlyCollection<ClinicalAllergyInput> Allergies);

    public sealed record AddClinicalNoteCommand(
        string NoteText);

    public interface IClinicalRecordCommandService
    {
        Task<ClinicalRecordDetailDto> CreateAsync(
            Guid patientId,
            SaveClinicalRecordSnapshotCommand command,
            CancellationToken cancellationToken = default);

        Task<ClinicalRecordDetailDto?> UpdateAsync(
            Guid patientId,
            SaveClinicalRecordSnapshotCommand command,
            CancellationToken cancellationToken = default);

        Task<ClinicalRecordDetailDto> AddNoteAsync(
            Guid patientId,
            AddClinicalNoteCommand command,
            CancellationToken cancellationToken = default);
    }

    public sealed class ClinicalRecordCommandService : IClinicalRecordCommandService
    {
        private readonly IClinicalRecordRepository _clinicalRecordRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public ClinicalRecordCommandService(
            IClinicalRecordRepository clinicalRecordRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _clinicalRecordRepository = clinicalRecordRepository ?? throw new ArgumentNullException(nameof(clinicalRecordRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<ClinicalRecordDetailDto> CreateAsync(
            Guid patientId,
            SaveClinicalRecordSnapshotCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var existingRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (existingRecord is not null)
            {
                throw new InvalidOperationException("A clinical record already exists for the requested patient.");
            }

            var clinicalRecord = new ClinicalRecord(
                tenantId,
                patient.Id,
                actorUserId,
                command.MedicalBackgroundSummary,
                command.CurrentMedicationsSummary,
                command.Allergies.Select(ToDraft));

            await _clinicalRecordRepository.AddAsync(clinicalRecord, cancellationToken);
            return clinicalRecord.ToDetailDto();
        }

        public async Task<ClinicalRecordDetailDto?> UpdateAsync(
            Guid patientId,
            SaveClinicalRecordSnapshotCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (clinicalRecord is null)
            {
                return null;
            }

            clinicalRecord.UpdateSnapshot(
                command.MedicalBackgroundSummary,
                command.CurrentMedicationsSummary,
                actorUserId);
            clinicalRecord.ReplaceAllergies(command.Allergies.Select(ToDraft), actorUserId);

            await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);
            return clinicalRecord.ToDetailDto();
        }

        public async Task<ClinicalRecordDetailDto> AddNoteAsync(
            Guid patientId,
            AddClinicalNoteCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (clinicalRecord is null)
            {
                throw new InvalidOperationException("The clinical record must be created explicitly before adding notes.");
            }

            clinicalRecord.AddClinicalNote(command.NoteText, actorUserId);
            await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);

            return clinicalRecord.ToDetailDto();
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
                throw new InvalidOperationException("Clinical records can only reference patients from the current tenant.");
            }
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Clinical record operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Clinical record write operations require a resolved user context.");
            }

            return userId;
        }

        private static ClinicalAllergyDraft ToDraft(ClinicalAllergyInput allergy)
        {
            return new ClinicalAllergyDraft(allergy.Substance, allergy.ReactionSummary, allergy.Notes);
        }
    }
}
