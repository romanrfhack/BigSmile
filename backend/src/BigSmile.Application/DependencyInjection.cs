using BigSmile.Application.Authorization;
using BigSmile.Application.Features.Branches.Queries;
using BigSmile.Application.Features.Tenants.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace BigSmile.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services (query handlers, command handlers, etc.)
            services.AddSingleton<IRolePermissionCatalog, RolePermissionCatalog>();
            services.AddScoped<IBranchQueryService, BranchQueryService>();
            services.AddScoped<ITenantQueryService, TenantQueryService>();

            return services;
        }
    }
}
