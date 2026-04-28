using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfReminderTemplateRepository : IReminderTemplateRepository
    {
        private readonly AppDbContext _dbContext;

        public EfReminderTemplateRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<ReminderTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.ReminderTemplates
                .FirstOrDefaultAsync(template => template.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<ReminderTemplate>> ListAsync(
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ReminderTemplates
                .Where(template => includeInactive || template.IsActive)
                .OrderBy(template => template.Name)
                .ThenBy(template => template.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(ReminderTemplate template, CancellationToken cancellationToken = default)
        {
            _dbContext.ReminderTemplates.Add(template);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(ReminderTemplate template, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(template).State == EntityState.Detached)
            {
                _dbContext.ReminderTemplates.Update(template);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
