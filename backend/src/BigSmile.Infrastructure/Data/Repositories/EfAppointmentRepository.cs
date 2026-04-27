using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfAppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _dbContext;

        public EfAppointmentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(appointment => appointment.Patient)
                .Include(appointment => appointment.Branch)
                .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Appointment>> GetCalendarAsync(
            Guid branchId,
            DateTime rangeStart,
            DateTime rangeEnd,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(appointment => appointment.Patient)
                .Where(appointment =>
                    appointment.BranchId == branchId &&
                    appointment.StartsAt < rangeEnd &&
                    appointment.EndsAt > rangeStart)
                .OrderBy(appointment => appointment.StartsAt)
                .ThenBy(appointment => appointment.Patient.LastName)
                .ThenBy(appointment => appointment.Patient.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Appointment>> GetManualRemindersAsync(
            Guid branchId,
            bool includeCompleted,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(appointment => appointment.Patient)
                .Where(appointment =>
                    appointment.BranchId == branchId &&
                    appointment.ReminderRequired &&
                    (includeCompleted || appointment.ReminderCompletedAtUtc == null))
                .OrderBy(appointment => appointment.ReminderDueAtUtc)
                .ThenBy(appointment => appointment.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            _dbContext.Appointments.Add(appointment);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(appointment).State == EntityState.Detached)
            {
                _dbContext.Appointments.Update(appointment);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
