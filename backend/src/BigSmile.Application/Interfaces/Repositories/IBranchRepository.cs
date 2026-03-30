using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IBranchRepository
    {
        Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Branch>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
        Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
        Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}