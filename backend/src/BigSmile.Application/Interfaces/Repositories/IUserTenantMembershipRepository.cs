using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IUserTenantMembershipRepository
    {
        Task<UserTenantMembership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserTenantMembership?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserTenantMembership>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserTenantMembership> AddAsync(UserTenantMembership membership, CancellationToken cancellationToken = default);
        Task UpdateAsync(UserTenantMembership membership, CancellationToken cancellationToken = default);
    }
}