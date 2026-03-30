using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Context;
using Microsoft.Extensions.DependencyInjection;

namespace BigSmile.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddSingleton<ITenantRepository, InMemoryTenantRepository>();
            services.AddSingleton<IBranchRepository, InMemoryBranchRepository>();

            // Register context as scoped (per request)
            services.AddScoped<TenantContext>();
            services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

            return services;
        }
    }
}