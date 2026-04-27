using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Appointment>> GetCalendarAsync(
            Guid branchId,
            DateTime rangeStart,
            DateTime rangeEnd,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Appointment>> GetManualRemindersAsync(
            Guid branchId,
            bool includeCompleted,
            CancellationToken cancellationToken = default);
        Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);
        Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default);
    }
}
