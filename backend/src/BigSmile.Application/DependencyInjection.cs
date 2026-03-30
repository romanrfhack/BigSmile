using BigSmile.Application.Features.Tenants.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace BigSmile.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services (query handlers, command handlers, etc.)
            services.AddScoped<ITenantQueryService, TenantQueryService>();

            return services;
        }
    }
}