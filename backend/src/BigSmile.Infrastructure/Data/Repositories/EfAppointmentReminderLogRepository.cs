using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfAppointmentReminderLogRepository : IAppointmentReminderLogRepository
    {
        private readonly AppDbContext _dbContext;

        public EfAppointmentReminderLogRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<AppointmentReminderLogEntry>> GetByAppointmentIdAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.AppointmentReminderLogEntries
                .Where(entry => entry.AppointmentId == appointmentId)
                .OrderByDescending(entry => entry.CreatedAtUtc)
                .ThenByDescending(entry => entry.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(AppointmentReminderLogEntry entry, CancellationToken cancellationToken = default)
        {
            _dbContext.AppointmentReminderLogEntries.Add(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
