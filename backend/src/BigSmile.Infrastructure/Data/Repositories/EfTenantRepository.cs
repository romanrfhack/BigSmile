using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public class EfTenantRepository : ITenantRepository
    {
        private readonly AppDbContext _dbContext;

        public EfTenantRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Tenants
                .Include(t => t.Branches)
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        }

        public async Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Tenants
                .Include(t => t.Branches)
                .FirstOrDefaultAsync(t => t.Subdomain == subdomain, cancellationToken);
        }

        public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Tenants
                .Include(t => t.Branches)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(tenant).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
            if (tenant != null)
            {
                _dbContext.Tenants.Remove(tenant);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}