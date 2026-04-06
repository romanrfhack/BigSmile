using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public class EfBranchRepository : IBranchRepository
    {
        private readonly AppDbContext _dbContext;

        public EfBranchRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Branches
                .Include(b => b.Tenant)
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Branch>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Branches
                .Where(b => b.TenantId == tenantId)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            _dbContext.Branches.Add(branch);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(branch).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var branch = await _dbContext.Branches.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (branch != null)
            {
                _dbContext.Branches.Remove(branch);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}