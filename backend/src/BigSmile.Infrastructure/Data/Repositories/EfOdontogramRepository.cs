using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfOdontogramRepository : IOdontogramRepository
    {
        private readonly AppDbContext _dbContext;

        public EfOdontogramRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Odontogram?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Odontograms
                .Include(odontogram => odontogram.Teeth)
                .SingleOrDefaultAsync(odontogram => odontogram.PatientId == patientId, cancellationToken);
        }

        public async Task AddAsync(Odontogram odontogram, CancellationToken cancellationToken = default)
        {
            _dbContext.Odontograms.Add(odontogram);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Odontogram odontogram, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(odontogram).State == EntityState.Detached)
            {
                _dbContext.Odontograms.Attach(odontogram);
                _dbContext.Entry(odontogram).State = EntityState.Modified;
            }

            var persistedToothIds = await _dbContext.OdontogramToothStates
                .AsNoTracking()
                .Where(tooth => tooth.OdontogramId == odontogram.Id)
                .Select(tooth => tooth.Id)
                .ToListAsync(cancellationToken);

            foreach (var tooth in odontogram.Teeth)
            {
                var entry = _dbContext.Entry(tooth);
                if (!persistedToothIds.Contains(tooth.Id))
                {
                    entry.State = EntityState.Added;
                    continue;
                }

                if (entry.State == EntityState.Detached)
                {
                    _dbContext.OdontogramToothStates.Attach(tooth);
                    _dbContext.Entry(tooth).State = EntityState.Modified;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
