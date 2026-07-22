using BigSmile.Application.Features.Tenants.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Tenants.Queries
{
    public interface ITenantQueryService
    {
        Task<TenantDto?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
    }

    public class TenantQueryService : ITenantQueryService
    {
        private readonly ITenantRepository _tenantRepository;

        public TenantQueryService(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        }

        public async Task<TenantDto?> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
            if (tenant == null)
                return null;

            return Map(tenant);
        }

        public async Task<IReadOnlyList<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
        {
            var tenants = await _tenantRepository.GetAllAsync(cancellationToken);
            return tenants
                .Select(Map)
                .ToList();
        }

        private static TenantDto Map(Tenant tenant)
        {
            return new TenantDto(
                tenant.Id,
                tenant.Name,
                tenant.Subdomain,
                tenant.TimeZoneId,
                tenant.IsActive,
                tenant.CreatedAt,
                tenant.UpdatedAt);
        }
    }
}
