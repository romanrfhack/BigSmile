using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Scheduling.Commands
{
    public sealed record AddAppointmentReminderLogEntryCommand(
        string Channel,
        string Outcome,
        string? Notes);

    public interface IAppointmentReminderLogCommandService
    {
        Task<AppointmentReminderLogEntryDto?> AddAsync(
            Guid appointmentId,
            AddAppointmentReminderLogEntryCommand command,
            CancellationToken cancellationToken = default);
    }

    public sealed class AppointmentReminderLogCommandService : IAppointmentReminderLogCommandService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentReminderLogRepository _reminderLogRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly ITenantContext _tenantContext;

        public AppointmentReminderLogCommandService(
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

        public async Task<AppointmentReminderLogEntryDto?> AddAsync(
            Guid appointmentId,
            AddAppointmentReminderLogEntryCommand command,
            CancellationToken cancellationToken = default)
        {
            GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment == null)
            {
                return null;
            }

            await GetRequiredActiveBranchAsync(appointment.BranchId, cancellationToken);

            var entry = appointment.AddReminderLogEntry(
                ParseChannel(command.Channel),
                ParseOutcome(command.Outcome),
                command.Notes,
                actorUserId);

            await _reminderLogRepository.AddAsync(entry, cancellationToken);
            return entry.ToDto();
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
                throw new InvalidOperationException("Appointment reminder log entries can only be managed in active branches.");
            }

            return branch;
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Scheduling reminder log operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Scheduling reminder log write operations require a resolved user context.");
            }

            return userId;
        }

        private static AppointmentReminderChannel ParseChannel(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
            {
                throw new ArgumentException("Appointment reminder channel is required.", nameof(channel));
            }

            if (!Enum.TryParse<AppointmentReminderChannel>(channel.Trim(), ignoreCase: true, out var parsedChannel) ||
                !Enum.IsDefined(parsedChannel))
            {
                throw new ArgumentException(
                    "Appointment reminder channel must be one of: Phone, WhatsApp, Email or Other.",
                    nameof(channel));
            }

            return parsedChannel;
        }

        private static AppointmentReminderOutcome ParseOutcome(string outcome)
        {
            if (string.IsNullOrWhiteSpace(outcome))
            {
                throw new ArgumentException("Appointment reminder outcome is required.", nameof(outcome));
            }

            if (!Enum.TryParse<AppointmentReminderOutcome>(outcome.Trim(), ignoreCase: true, out var parsedOutcome) ||
                !Enum.IsDefined(parsedOutcome))
            {
                throw new ArgumentException(
                    "Appointment reminder outcome must be one of: Reached, NoAnswer or LeftMessage.",
                    nameof(outcome));
            }

            return parsedOutcome;
        }
    }
}
