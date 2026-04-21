using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfTreatmentPlanRepository : ITreatmentPlanRepository
    {
        private readonly AppDbContext _dbContext;

        public EfTreatmentPlanRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TreatmentPlan?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TreatmentPlans
                .Include(treatmentPlan => treatmentPlan.Items)
                .SingleOrDefaultAsync(treatmentPlan => treatmentPlan.PatientId == patientId, cancellationToken);
        }

        public async Task AddAsync(TreatmentPlan treatmentPlan, CancellationToken cancellationToken = default)
        {
            _dbContext.TreatmentPlans.Add(treatmentPlan);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TreatmentPlan treatmentPlan, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(treatmentPlan).State == EntityState.Detached)
            {
                _dbContext.TreatmentPlans.Attach(treatmentPlan);
                _dbContext.Entry(treatmentPlan).State = EntityState.Modified;
            }

            var persistedItemIds = await _dbContext.TreatmentPlanItems
                .AsNoTracking()
                .Where(item => item.TreatmentPlanId == treatmentPlan.Id)
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);

            MarkNewChildrenAsAdded(treatmentPlan.Items, persistedItemIds);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private void MarkNewChildrenAsAdded(IEnumerable<TreatmentPlanItem> children, IReadOnlyCollection<Guid> persistedIds)
        {
            foreach (var child in children)
            {
                if (child is not Entity<Guid> entity)
                {
                    continue;
                }

                var entry = _dbContext.Entry(child);
                if (!persistedIds.Contains(entity.Id))
                {
                    entry.State = EntityState.Added;
                }
            }
        }
    }
}
