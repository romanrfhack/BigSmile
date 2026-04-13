using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IAppointmentBlockRepository
    {
        Task<AppointmentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AppointmentBlock>> GetCalendarAsync(
            Guid branchId,
            DateTime rangeStart,
            DateTime rangeEnd,
            CancellationToken cancellationToken = default);
        Task<bool> ExistsOverlappingAsync(
            Guid branchId,
            DateTime startsAt,
            DateTime endsAt,
            CancellationToken cancellationToken = default);
        Task AddAsync(AppointmentBlock appointmentBlock, CancellationToken cancellationToken = default);
        Task DeleteAsync(AppointmentBlock appointmentBlock, CancellationToken cancellationToken = default);
    }
}
