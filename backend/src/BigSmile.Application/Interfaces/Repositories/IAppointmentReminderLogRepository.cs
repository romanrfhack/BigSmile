using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IAppointmentReminderLogRepository
    {
        Task<IReadOnlyList<AppointmentReminderLogEntry>> GetByAppointmentIdAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default);

        Task AddAsync(AppointmentReminderLogEntry entry, CancellationToken cancellationToken = default);
    }
}
