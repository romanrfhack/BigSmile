using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BigSmile.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register EF-based repositories (scoped, as they depend on DbContext)
            services.AddScoped<ITenantRepository, EfTenantRepository>();
            services.AddScoped<IBranchRepository, EfBranchRepository>();
            services.AddScoped<IPatientRepository, EfPatientRepository>();
            services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
            services.AddScoped<IAppointmentBlockRepository, EfAppointmentBlockRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IRoleRepository, EfRoleRepository>();
            services.AddScoped<IUserTenantMembershipRepository, EfUserTenantMembershipRepository>();

            // Register context as scoped (per request)
            services.AddScoped<TenantContext>();
            services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

            // Register JWT token service
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            // Register password hasher
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}
