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
                .Include(odontogram => odontogram.Surfaces)
                .Include(odontogram => odontogram.SurfaceFindings)
                .Include(odontogram => odontogram.SurfaceFindingHistoryEntries)
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
                .ToHashSetAsync(cancellationToken);

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

            var persistedSurfaceIds = await _dbContext.OdontogramSurfaceStates
                .AsNoTracking()
                .Where(surface => surface.OdontogramId == odontogram.Id)
                .Select(surface => surface.Id)
                .ToHashSetAsync(cancellationToken);

            foreach (var surface in odontogram.Surfaces)
            {
                var entry = _dbContext.Entry(surface);
                if (!persistedSurfaceIds.Contains(surface.Id))
                {
                    entry.State = EntityState.Added;
                    continue;
                }

                if (entry.State == EntityState.Detached)
                {
                    _dbContext.OdontogramSurfaceStates.Attach(surface);
                    _dbContext.Entry(surface).State = EntityState.Modified;
                }
            }

            var persistedFindingIds = await _dbContext.OdontogramSurfaceFindings
                .AsNoTracking()
                .Where(finding => finding.OdontogramId == odontogram.Id)
                .Select(finding => finding.Id)
                .ToHashSetAsync(cancellationToken);

            foreach (var finding in odontogram.SurfaceFindings)
            {
                var entry = _dbContext.Entry(finding);
                if (!persistedFindingIds.Contains(finding.Id))
                {
                    entry.State = EntityState.Added;
                }
            }

            var persistedHistoryIds = await _dbContext.OdontogramSurfaceFindingHistoryEntries
                .AsNoTracking()
                .Where(entry => entry.OdontogramId == odontogram.Id)
                .Select(entry => entry.Id)
                .ToHashSetAsync(cancellationToken);

            foreach (var historyEntry in odontogram.SurfaceFindingHistoryEntries)
            {
                var entry = _dbContext.Entry(historyEntry);
                if (!persistedHistoryIds.Contains(historyEntry.Id))
                {
                    entry.State = EntityState.Added;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
