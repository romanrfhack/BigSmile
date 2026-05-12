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

    public sealed record AddClinicalDiagnosisCommand(
        string DiagnosisText,
        string? Notes);

    public sealed record SaveClinicalMedicalAnswerCommand(
        string QuestionKey,
        ClinicalMedicalAnswerValue Answer,
        string? Details);

    public sealed record SaveClinicalMedicalQuestionnaireCommand(
        IReadOnlyCollection<SaveClinicalMedicalAnswerCommand> Answers);

    public sealed record CreateClinicalEncounterCommand(
        DateTime OccurredAtUtc,
        string ChiefComplaint,
        ClinicalEncounterConsultationType ConsultationType,
        decimal? TemperatureC,
        int? BloodPressureSystolic,
        int? BloodPressureDiastolic,
        decimal? WeightKg,
        decimal? HeightCm,
        int? RespiratoryRatePerMinute,
        int? HeartRateBpm,
        string? NoteText);

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

        Task<ClinicalRecordDetailDto> AddDiagnosisAsync(
            Guid patientId,
            AddClinicalDiagnosisCommand command,
            CancellationToken cancellationToken = default);

        Task<ClinicalRecordDetailDto> ResolveDiagnosisAsync(
            Guid patientId,
            Guid diagnosisId,
            CancellationToken cancellationToken = default);

        Task<ClinicalMedicalQuestionnaireDto> UpdateQuestionnaireAsync(
            Guid patientId,
            SaveClinicalMedicalQuestionnaireCommand command,
            CancellationToken cancellationToken = default);

        Task<ClinicalEncounterDto> CreateEncounterAsync(
            Guid patientId,
            CreateClinicalEncounterCommand command,
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

            var snapshotChanged = clinicalRecord.ApplySnapshot(
                command.MedicalBackgroundSummary,
                command.CurrentMedicationsSummary,
                command.Allergies.Select(ToDraft),
                actorUserId);

            if (snapshotChanged)
            {
                await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);
            }

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

        public async Task<ClinicalRecordDetailDto> AddDiagnosisAsync(
            Guid patientId,
            AddClinicalDiagnosisCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (clinicalRecord is null)
            {
                throw new InvalidOperationException("The clinical record must be created explicitly before adding diagnoses.");
            }

            clinicalRecord.AddDiagnosis(command.DiagnosisText, command.Notes, actorUserId);
            await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);

            return clinicalRecord.ToDetailDto();
        }

        public async Task<ClinicalRecordDetailDto> ResolveDiagnosisAsync(
            Guid patientId,
            Guid diagnosisId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (clinicalRecord is null)
            {
                throw new InvalidOperationException("The clinical record must exist before resolving diagnoses.");
            }

            clinicalRecord.ResolveDiagnosis(diagnosisId, actorUserId);
            await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);

            return clinicalRecord.ToDetailDto();
        }

        public async Task<ClinicalMedicalQuestionnaireDto> UpdateQuestionnaireAsync(
            Guid patientId,
            SaveClinicalMedicalQuestionnaireCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (clinicalRecord is null)
            {
                throw new InvalidOperationException("The clinical record must be created explicitly before updating the medical questionnaire.");
            }

            EnsureClinicalRecordBelongsToTenantAndPatient(clinicalRecord, tenantId, patient.Id);

            var questionnaireChanged = clinicalRecord.UpsertMedicalAnswers(
                command.Answers.Select(ToDraft),
                actorUserId);

            if (questionnaireChanged)
            {
                await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);
            }

            return clinicalRecord.ToQuestionnaireDto();
        }

        public async Task<ClinicalEncounterDto> CreateEncounterAsync(
            Guid patientId,
            CreateClinicalEncounterCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var clinicalRecord = await _clinicalRecordRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (clinicalRecord is null)
            {
                throw new InvalidOperationException("The clinical record must be created explicitly before adding encounters.");
            }

            EnsureClinicalRecordBelongsToTenantAndPatient(clinicalRecord, tenantId, patient.Id);

            var encounter = clinicalRecord.AddEncounter(
                command.OccurredAtUtc,
                command.ChiefComplaint,
                command.ConsultationType,
                command.TemperatureC,
                command.BloodPressureSystolic,
                command.BloodPressureDiastolic,
                command.WeightKg,
                command.HeightCm,
                command.RespiratoryRatePerMinute,
                command.HeartRateBpm,
                command.NoteText,
                actorUserId);

            await _clinicalRecordRepository.UpdateAsync(clinicalRecord, cancellationToken);

            return encounter.ToDto();
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

        private static void EnsureClinicalRecordBelongsToTenantAndPatient(
            ClinicalRecord clinicalRecord,
            Guid tenantId,
            Guid patientId)
        {
            if (clinicalRecord.TenantId != tenantId)
            {
                throw new InvalidOperationException("The clinical record is not available in the current tenant scope.");
            }

            if (clinicalRecord.PatientId != patientId)
            {
                throw new InvalidOperationException("The clinical record does not belong to the requested patient.");
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

        private static ClinicalMedicalAnswerDraft ToDraft(SaveClinicalMedicalAnswerCommand answer)
        {
            return new ClinicalMedicalAnswerDraft(answer.QuestionKey, answer.Answer, answer.Details);
        }
    }
}
