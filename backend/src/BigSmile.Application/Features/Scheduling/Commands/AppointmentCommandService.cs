using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Commands
{
    public sealed record CreateAppointmentCommand(
        Guid BranchId,
        Guid PatientId,
        DateTime StartsAt,
        DateTime EndsAt,
        string? Notes);

    public sealed record UpdateAppointmentCommand(
        Guid PatientId,
        DateTime StartsAt,
        DateTime EndsAt,
        string? Notes);

    public sealed record RescheduleAppointmentCommand(
        DateTime StartsAt,
        DateTime EndsAt);

    public sealed record CancelAppointmentCommand(
        string? Reason);

    public interface IAppointmentCommandService
    {
        Task<AppointmentSummaryDto> CreateAsync(CreateAppointmentCommand command, CancellationToken cancellationToken = default);
        Task<AppointmentSummaryDto?> UpdateAsync(Guid id, UpdateAppointmentCommand command, CancellationToken cancellationToken = default);
        Task<AppointmentSummaryDto?> RescheduleAsync(Guid id, RescheduleAppointmentCommand command, CancellationToken cancellationToken = default);
        Task<AppointmentSummaryDto?> CancelAsync(Guid id, CancelAppointmentCommand command, CancellationToken cancellationToken = default);
    }

    public sealed class AppointmentCommandService : IAppointmentCommandService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentBlockRepository _appointmentBlockRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly ITenantContext _tenantContext;

        public AppointmentCommandService(
            IAppointmentRepository appointmentRepository,
            IAppointmentBlockRepository appointmentBlockRepository,
            IPatientRepository patientRepository,
            IBranchAccessService branchAccessService,
            ITenantContext tenantContext)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _appointmentBlockRepository = appointmentBlockRepository ?? throw new ArgumentNullException(nameof(appointmentBlockRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<AppointmentSummaryDto> CreateAsync(CreateAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var branch = await GetRequiredActiveBranchAsync(command.BranchId, cancellationToken);
            var patient = await GetRequiredPatientAsync(command.PatientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);
            await EnsureNoAppointmentBlockConflictAsync(
                branch.Id,
                command.StartsAt,
                command.EndsAt,
                cancellationToken);

            var appointment = new Appointment(
                tenantId,
                branch.Id,
                patient.Id,
                command.StartsAt,
                command.EndsAt,
                command.Notes);

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            return await GetRequiredSummaryAsync(appointment.Id, cancellationToken);
        }

        public async Task<AppointmentSummaryDto?> UpdateAsync(Guid id, UpdateAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

            var branch = await GetRequiredActiveBranchAsync(appointment.BranchId, cancellationToken);
            var patient = await GetRequiredPatientAsync(command.PatientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, branch.TenantId);
            if (HasScheduleChanged(appointment, command.StartsAt, command.EndsAt))
            {
                await EnsureNoAppointmentBlockConflictAsync(
                    appointment.BranchId,
                    command.StartsAt,
                    command.EndsAt,
                    cancellationToken);
            }

            appointment.Update(
                patient.Id,
                command.StartsAt,
                command.EndsAt,
                command.Notes);

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            return await GetRequiredSummaryAsync(appointment.Id, cancellationToken);
        }

        public async Task<AppointmentSummaryDto?> RescheduleAsync(Guid id, RescheduleAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

            await GetRequiredActiveBranchAsync(appointment.BranchId, cancellationToken);
            if (HasScheduleChanged(appointment, command.StartsAt, command.EndsAt))
            {
                await EnsureNoAppointmentBlockConflictAsync(
                    appointment.BranchId,
                    command.StartsAt,
                    command.EndsAt,
                    cancellationToken);
            }

            appointment.Reschedule(command.StartsAt, command.EndsAt);
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            return await GetRequiredSummaryAsync(appointment.Id, cancellationToken);
        }

        public async Task<AppointmentSummaryDto?> CancelAsync(Guid id, CancelAppointmentCommand command, CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

            await GetRequiredActiveBranchAsync(appointment.BranchId, cancellationToken);

            appointment.Cancel(command.Reason);
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            return await GetRequiredSummaryAsync(appointment.Id, cancellationToken);
        }

        private async Task<AppointmentSummaryDto> GetRequiredSummaryAsync(Guid appointmentId, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                throw new InvalidOperationException("The appointment could not be reloaded after saving.");
            }

            return appointment.ToSummaryDto();
        }

        private async Task<Branch> GetRequiredActiveBranchAsync(Guid branchId, CancellationToken cancellationToken)
        {
            var branch = await _branchAccessService.GetAccessibleBranchAsync(branchId, cancellationToken);
            if (branch == null)
            {
                throw new InvalidOperationException("The requested branch is not accessible in the current tenant scope.");
            }

            if (!branch.IsActive)
            {
                throw new InvalidOperationException("Appointments can only be managed in active branches.");
            }

            return branch;
        }

        private async Task<Patient> GetRequiredPatientAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient == null)
            {
                throw new InvalidOperationException("The requested patient is not available in the current tenant scope.");
            }

            return patient;
        }

        private static void EnsurePatientBelongsToTenant(Patient patient, Guid tenantId)
        {
            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException("Appointments can only reference patients from the current tenant.");
            }
        }

        private async Task EnsureNoAppointmentBlockConflictAsync(
            Guid branchId,
            DateTime startsAt,
            DateTime endsAt,
            CancellationToken cancellationToken)
        {
            var overlapsBlockedSlot = await _appointmentBlockRepository.ExistsOverlappingAsync(
                branchId,
                startsAt,
                endsAt,
                cancellationToken);

            if (overlapsBlockedSlot)
            {
                throw new InvalidOperationException(
                    "Appointments cannot be scheduled inside blocked time slots for the selected branch.");
            }
        }

        private static bool HasScheduleChanged(Appointment appointment, DateTime startsAt, DateTime endsAt)
        {
            return appointment.StartsAt != startsAt || appointment.EndsAt != endsAt;
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Scheduling operations require a resolved tenant context.");
            }

            return tenantId;
        }
    }
}
