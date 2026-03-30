using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface ITenantRepository
    {
        Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
        Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}