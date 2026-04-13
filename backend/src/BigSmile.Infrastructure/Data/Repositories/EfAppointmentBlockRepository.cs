using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfAppointmentBlockRepository : IAppointmentBlockRepository
    {
        private readonly AppDbContext _dbContext;

        public EfAppointmentBlockRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<AppointmentBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.AppointmentBlocks
                .Include(block => block.Branch)
                .FirstOrDefaultAsync(block => block.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<AppointmentBlock>> GetCalendarAsync(
            Guid branchId,
            DateTime rangeStart,
            DateTime rangeEnd,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.AppointmentBlocks
                .Where(block =>
                    block.BranchId == branchId &&
                    block.StartsAt < rangeEnd &&
                    block.EndsAt > rangeStart)
                .OrderBy(block => block.StartsAt)
                .ThenBy(block => block.EndsAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsOverlappingAsync(
            Guid branchId,
            DateTime startsAt,
            DateTime endsAt,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.AppointmentBlocks
                .AnyAsync(block =>
                    block.BranchId == branchId &&
                    block.StartsAt < endsAt &&
                    block.EndsAt > startsAt,
                    cancellationToken);
        }

        public async Task AddAsync(AppointmentBlock appointmentBlock, CancellationToken cancellationToken = default)
        {
            _dbContext.AppointmentBlocks.Add(appointmentBlock);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(AppointmentBlock appointmentBlock, CancellationToken cancellationToken = default)
        {
            _dbContext.AppointmentBlocks.Remove(appointmentBlock);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
