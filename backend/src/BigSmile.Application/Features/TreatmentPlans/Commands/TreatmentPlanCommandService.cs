using BigSmile.Application.Features.TreatmentPlans.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.TreatmentPlans.Commands
{
    public sealed record AddTreatmentPlanItemCommand(
        string Title,
        string? Category,
        int Quantity,
        string? Notes,
        string? ToothCode,
        string? SurfaceCode);

    public sealed record ChangeTreatmentPlanStatusCommand(
        string Status);

    public interface ITreatmentPlanCommandService
    {
        Task<TreatmentPlanDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task<TreatmentPlanDetailDto?> AddItemAsync(
            Guid patientId,
            AddTreatmentPlanItemCommand command,
            CancellationToken cancellationToken = default);

        Task<TreatmentPlanDetailDto?> RemoveItemAsync(
            Guid patientId,
            Guid itemId,
            CancellationToken cancellationToken = default);

        Task<TreatmentPlanDetailDto?> ChangeStatusAsync(
            Guid patientId,
            ChangeTreatmentPlanStatusCommand command,
            CancellationToken cancellationToken = default);
    }

    public sealed class TreatmentPlanCommandService : ITreatmentPlanCommandService
    {
        private readonly ITreatmentPlanRepository _treatmentPlanRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public TreatmentPlanCommandService(
            ITreatmentPlanRepository treatmentPlanRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _treatmentPlanRepository = treatmentPlanRepository ?? throw new ArgumentNullException(nameof(treatmentPlanRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<TreatmentPlanDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var existingPlan = await _treatmentPlanRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (existingPlan is not null)
            {
                throw new InvalidOperationException("A treatment plan already exists for the requested patient.");
            }

            var treatmentPlan = new TreatmentPlan(tenantId, patient.Id, actorUserId);
            await _treatmentPlanRepository.AddAsync(treatmentPlan, cancellationToken);
            return treatmentPlan.ToDetailDto();
        }

        public async Task<TreatmentPlanDetailDto?> AddItemAsync(
            Guid patientId,
            AddTreatmentPlanItemCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentPlan = await _treatmentPlanRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentPlan is null)
            {
                return null;
            }

            treatmentPlan.AddItem(
                command.Title,
                command.Category,
                command.Quantity,
                command.Notes,
                command.ToothCode,
                command.SurfaceCode,
                actorUserId);

            await _treatmentPlanRepository.UpdateAsync(treatmentPlan, cancellationToken);
            return treatmentPlan.ToDetailDto();
        }

        public async Task<TreatmentPlanDetailDto?> RemoveItemAsync(
            Guid patientId,
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentPlan = await _treatmentPlanRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentPlan is null)
            {
                return null;
            }

            treatmentPlan.RemoveItem(itemId, actorUserId);
            await _treatmentPlanRepository.UpdateAsync(treatmentPlan, cancellationToken);
            return treatmentPlan.ToDetailDto();
        }

        public async Task<TreatmentPlanDetailDto?> ChangeStatusAsync(
            Guid patientId,
            ChangeTreatmentPlanStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var treatmentPlan = await _treatmentPlanRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (treatmentPlan is null)
            {
                return null;
            }

            var newStatus = ParseStatus(command.Status);
            var changed = treatmentPlan.ChangeStatus(newStatus, actorUserId);
            if (changed)
            {
                await _treatmentPlanRepository.UpdateAsync(treatmentPlan, cancellationToken);
            }

            return treatmentPlan.ToDetailDto();
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
                throw new InvalidOperationException("Treatment plans can only reference patients from the current tenant.");
            }
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Treatment plan operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Treatment plan write operations require a resolved user context.");
            }

            return userId;
        }

        private static TreatmentPlanStatus ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Treatment plan status is required.", nameof(status));
            }

            if (!Enum.TryParse<TreatmentPlanStatus>(status.Trim(), ignoreCase: true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus))
            {
                throw new ArgumentException(
                    "Treatment plan status must be one of: Draft, Proposed, or Accepted.",
                    nameof(status));
            }

            return parsedStatus;
        }
    }
}
