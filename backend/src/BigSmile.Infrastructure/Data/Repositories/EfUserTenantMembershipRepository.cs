using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public class EfUserTenantMembershipRepository : IUserTenantMembershipRepository
    {
        private readonly AppDbContext _dbContext;

        public EfUserTenantMembershipRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserTenantMembership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserTenantMemberships
                .Include(m => m.User)
                .Include(m => m.Tenant)
                .Include(m => m.Role)
                .Include(m => m.BranchAssignments)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        }

        public async Task<UserTenantMembership?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserTenantMemberships
                .Include(m => m.User)
                .Include(m => m.Tenant)
                .Include(m => m.Role)
                .Include(m => m.BranchAssignments)
                .FirstOrDefaultAsync(m => m.UserId == userId && m.TenantId == tenantId, cancellationToken);
        }

        public async Task<IReadOnlyList<UserTenantMembership>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserTenantMemberships
                .Include(m => m.Tenant)
                .Include(m => m.Role)
                .Where(m => m.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<UserTenantMembership> AddAsync(UserTenantMembership membership, CancellationToken cancellationToken = default)
        {
            _dbContext.UserTenantMemberships.Add(membership);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return membership;
        }

        public async Task UpdateAsync(UserTenantMembership membership, CancellationToken cancellationToken = default)
        {
            _dbContext.Entry(membership).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}