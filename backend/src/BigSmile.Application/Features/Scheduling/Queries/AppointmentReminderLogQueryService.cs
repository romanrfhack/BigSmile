using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Queries
{
    public interface IAppointmentReminderLogQueryService
    {
        Task<IReadOnlyList<AppointmentReminderLogEntryDto>?> ListAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default);
    }

    public sealed class AppointmentReminderLogQueryService : IAppointmentReminderLogQueryService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentReminderLogRepository _reminderLogRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly ITenantContext _tenantContext;

        public AppointmentReminderLogQueryService(
            IAppointmentRepository appointmentRepository,
            IAppointmentReminderLogRepository reminderLogRepository,
            IBranchAccessService branchAccessService,
            ITenantContext tenantContext)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _reminderLogRepository = reminderLogRepository ?? throw new ArgumentNullException(nameof(reminderLogRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<IReadOnlyList<AppointmentReminderLogEntryDto>?> ListAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            EnsureTenantContext();

            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

            await GetRequiredActiveBranchAsync(appointment.BranchId, cancellationToken);

            var entries = await _reminderLogRepository.GetByAppointmentIdAsync(appointment.Id, cancellationToken);
            return entries.Select(entry => entry.ToDto()).ToArray();
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
                throw new InvalidOperationException("Appointment reminder log entries can only be read in active branches.");
            }

            return branch;
        }

        private void EnsureTenantContext()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Scheduling reminder log queries require a resolved tenant context.");
            }
        }
    }
}
