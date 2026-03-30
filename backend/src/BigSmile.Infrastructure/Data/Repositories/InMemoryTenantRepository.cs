using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public class InMemoryTenantRepository : ITenantRepository
    {
        private static readonly Dictionary<Guid, Tenant> _tenants = new();

        public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _tenants.TryGetValue(id, out var tenant);
            return Task.FromResult(tenant);
        }

        public Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
        {
            var tenant = _tenants.Values.FirstOrDefault(t => t.Subdomain == subdomain);
            return Task.FromResult(tenant);
        }

        public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Tenant> list = _tenants.Values.ToList();
            return Task.FromResult(list);
        }

        public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            if (_tenants.ContainsKey(tenant.Id))
                throw new InvalidOperationException($"Tenant with id {tenant.Id} already exists.");
            _tenants.Add(tenant.Id, tenant);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
        {
            if (!_tenants.ContainsKey(tenant.Id))
                throw new InvalidOperationException($"Tenant with id {tenant.Id} not found.");
            _tenants[tenant.Id] = tenant;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _tenants.Remove(id);
            return Task.CompletedTask;
        }
    }
}