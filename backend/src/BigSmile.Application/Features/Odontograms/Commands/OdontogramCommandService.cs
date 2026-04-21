using BigSmile.Application.Features.Odontograms.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Odontograms.Commands
{
    public sealed record UpdateOdontogramToothStatusCommand(
        string ToothCode,
        string Status);

    public interface IOdontogramCommandService
    {
        Task<OdontogramDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);

        Task<OdontogramDetailDto?> UpdateToothStatusAsync(
            Guid patientId,
            UpdateOdontogramToothStatusCommand command,
            CancellationToken cancellationToken = default);
    }

    public sealed class OdontogramCommandService : IOdontogramCommandService
    {
        private readonly IOdontogramRepository _odontogramRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public OdontogramCommandService(
            IOdontogramRepository odontogramRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _odontogramRepository = odontogramRepository ?? throw new ArgumentNullException(nameof(odontogramRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<OdontogramDetailDto> CreateAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var existingOdontogram = await _odontogramRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (existingOdontogram is not null)
            {
                throw new InvalidOperationException("An odontogram already exists for the requested patient.");
            }

            var odontogram = new Odontogram(tenantId, patient.Id, actorUserId);

            await _odontogramRepository.AddAsync(odontogram, cancellationToken);
            return odontogram.ToDetailDto();
        }

        public async Task<OdontogramDetailDto?> UpdateToothStatusAsync(
            Guid patientId,
            UpdateOdontogramToothStatusCommand command,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var odontogram = await _odontogramRepository.GetByPatientIdAsync(patientId, cancellationToken);
            if (odontogram is null)
            {
                return null;
            }

            var status = ParseStatus(command.Status);
            var changed = odontogram.UpdateToothStatus(command.ToothCode, status, actorUserId);
            if (changed)
            {
                await _odontogramRepository.UpdateAsync(odontogram, cancellationToken);
            }

            return odontogram.ToDetailDto();
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
                throw new InvalidOperationException("Odontograms can only reference patients from the current tenant.");
            }
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Odontogram operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Odontogram write operations require a resolved user context.");
            }

            return userId;
        }

        private static OdontogramToothStatus ParseStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Tooth status is required.", nameof(status));
            }

            if (!Enum.TryParse<OdontogramToothStatus>(status.Trim(), ignoreCase: true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus))
            {
                throw new ArgumentException(
                    "Tooth status must be one of: Unknown, Healthy, Missing, Restored, or Caries.",
                    nameof(status));
            }

            return parsedStatus;
        }
    }
}
