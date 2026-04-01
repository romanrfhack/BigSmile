using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BigSmile.Infrastructure.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseInitializer");
            var dbContext = services.GetRequiredService<AppDbContext>();

            // Ensure database is created and migrations applied
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrated successfully.");

            // Seed roles (already seeded via configuration, but ensure they exist)
            var roleRepo = services.GetRequiredService<IRoleRepository>();
            var tenantRepo = services.GetRequiredService<ITenantRepository>();
            var userRepo = services.GetRequiredService<IUserRepository>();
            var membershipRepo = services.GetRequiredService<IUserTenantMembershipRepository>();
            var passwordHasher = services.GetRequiredService<IPasswordHasher>();

            // Ensure system roles exist
            var platformAdminRole = await roleRepo.GetByNameAsync("PlatformAdmin");
            var tenantAdminRole = await roleRepo.GetByNameAsync("TenantAdmin");
            var tenantUserRole = await roleRepo.GetByNameAsync("TenantUser");

            if (platformAdminRole == null)
            {
                logger.LogError("System role PlatformAdmin not found. Did the seed migration run?");
                return;
            }

            // Create default tenant if none exists
            var defaultTenant = await tenantRepo.GetBySubdomainAsync("default");
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant("Default Tenant", "default");
                await tenantRepo.AddAsync(defaultTenant);
                logger.LogInformation("Default tenant created.");
            }

            // Create default admin user if none exists
            var adminEmail = "admin@bigsmile.local";
            var adminUser = await userRepo.GetByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var hashedPassword = passwordHasher.HashPassword("Admin123!");
                adminUser = new User(adminEmail, hashedPassword, "System Administrator");
                await userRepo.AddAsync(adminUser);
                logger.LogInformation("Default admin user created.");
            }

            // Ensure admin user is a PlatformAdmin member of default tenant
            var adminMembership = await membershipRepo.GetByUserAndTenantAsync(adminUser.Id, defaultTenant.Id);
            if (adminMembership == null)
            {
                adminMembership = adminUser.AddTenantMembership(defaultTenant, platformAdminRole);
                await membershipRepo.AddAsync(adminMembership);
                logger.LogInformation("Admin user assigned as PlatformAdmin to default tenant.");
            }
        }
    }
}