using BigSmile.Application.Authorization;
using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.Scheduling
{
    public class AppointmentBlockServicesTests
    {
        [Fact]
        public async Task CreateAsync_CreatesBlockedSlotInsideResolvedTenantAndAssignedBranch()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateBlockCommandService(context, tenantContext);

            var block = await commandService.CreateAsync(new CreateAppointmentBlockCommand(
                seedData.PrimaryBranch.Id,
                new DateTime(2026, 4, 16, 13, 0, 0),
                new DateTime(2026, 4, 16, 14, 0, 0),
                "Lunch break"));

            Assert.Equal(seedData.PrimaryBranch.Id, block.BranchId);
            Assert.Equal("Lunch break", block.Label);

            await using var verificationContext = CreateContext(databaseName, new TenantContext());
            var storedBlock = await verificationContext.AppointmentBlocks.SingleAsync();

            Assert.Equal(seedData.Tenant.Id, storedBlock.TenantId);
            Assert.Equal(seedData.PrimaryBranch.Id, storedBlock.BranchId);
        }

        [Fact]
        public async Task GetCalendarAsync_ReturnsOnlyCurrentTenantBranchBlockedSlots()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");

            await SeedAppointmentBlockAsync(
                databaseName,
                tenantASeed.Tenant.Id,
                tenantASeed.PrimaryBranch.Id,
                new DateTime(2026, 4, 16, 13, 0, 0),
                new DateTime(2026, 4, 16, 14, 0, 0),
                "Tenant A lunch");
            await SeedAppointmentBlockAsync(
                databaseName,
                tenantBSeed.Tenant.Id,
                tenantBSeed.PrimaryBranch.Id,
                new DateTime(2026, 4, 16, 15, 0, 0),
                new DateTime(2026, 4, 16, 16, 0, 0),
                "Tenant B maintenance");

            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);
            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var calendar = await queryService.GetCalendarAsync(
                tenantASeed.PrimaryBranch.Id,
                new DateOnly(2026, 4, 16),
                1);

            Assert.Single(calendar.CalendarDays);
            Assert.Single(calendar.CalendarDays[0].BlockedSlots);
            Assert.Equal("Tenant A lunch", calendar.CalendarDays[0].BlockedSlots[0].Label);
            Assert.Empty(calendar.CalendarDays[0].Appointments);
        }

        [Fact]
        public async Task GetCalendarAsync_BlocksCrossTenantBlockedSlotAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");

            await SeedAppointmentBlockAsync(
                databaseName,
                tenantBSeed.Tenant.Id,
                tenantBSeed.PrimaryBranch.Id,
                new DateTime(2026, 4, 16, 15, 0, 0),
                new DateTime(2026, 4, 16, 16, 0, 0),
                "Tenant B maintenance");

            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);
            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.GetCalendarAsync(
                tenantBSeed.PrimaryBranch.Id,
                new DateOnly(2026, 4, 16),
                1));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_BlocksBranchOutsideAssignedScope_ForTenantUser()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateBlockCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(
                new CreateAppointmentBlockCommand(
                    seedData.SecondaryBranch.Id,
                    new DateTime(2026, 4, 16, 13, 0, 0),
                    new DateTime(2026, 4, 16, 14, 0, 0),
                    "Lunch break")));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DeleteAsync_RemovesExistingBlockedSlotInsideAccessibleBranch()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var appointmentBlock = await SeedAppointmentBlockAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                new DateTime(2026, 4, 16, 13, 0, 0),
                new DateTime(2026, 4, 16, 14, 0, 0),
                "Lunch break");
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateBlockCommandService(context, tenantContext);

            var deleted = await commandService.DeleteAsync(appointmentBlock.Id);

            Assert.True(deleted);

            await using var verificationContext = CreateContext(databaseName, new TenantContext());
            Assert.Empty(await verificationContext.AppointmentBlocks.ToListAsync());
        }

        private static AppointmentBlockCommandService CreateBlockCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new AppointmentBlockCommandService(
                new EfAppointmentBlockRepository(context),
                new BranchAccessService(
                    new EfBranchRepository(context),
                    new EfUserTenantMembershipRepository(context),
                    new RolePermissionCatalog(),
                    tenantContext),
                tenantContext);
        }

        private static AppointmentQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new AppointmentQueryService(
                new EfAppointmentRepository(context),
                new EfAppointmentBlockRepository(context),
                new BranchAccessService(
                    new EfBranchRepository(context),
                    new EfUserTenantMembershipRepository(context),
                    new RolePermissionCatalog(),
                    tenantContext),
                tenantContext);
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(userId.ToString(), AccessScope.Tenant, isAuthenticated: true, tenantId.ToString());
            return tenantContext;
        }

        private static async Task<SeedData> SeedTenantWithUserAsync(
            string databaseName,
            string tenantName = "Tenant A",
            string tenantSubdomain = "tenant-a")
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenant = new Tenant(tenantName, tenantSubdomain);
            var primaryBranch = tenant.AddBranch($"{tenantName} Main");
            var secondaryBranch = tenant.AddBranch($"{tenantName} Secondary");
            var role = new Role(SystemRoles.TenantUser);
            var user = new User($"{tenantSubdomain}@example.com", "hashed-password", $"{tenantName} User");
            var membership = user.AddTenantMembership(tenant, role);
            membership.AssignToBranch(primaryBranch);

            context.Tenants.Add(tenant);
            context.Roles.Add(role);
            context.Users.Add(user);
            context.UserTenantMemberships.Add(membership);
            await context.SaveChangesAsync();

            return new SeedData(tenant, user, primaryBranch, secondaryBranch);
        }

        private static async Task<AppointmentBlock> SeedAppointmentBlockAsync(
            string databaseName,
            Guid tenantId,
            Guid branchId,
            DateTime startsAt,
            DateTime endsAt,
            string? label)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointmentBlock = new AppointmentBlock(tenantId, branchId, startsAt, endsAt, label);

            context.AppointmentBlocks.Add(appointmentBlock);
            await context.SaveChangesAsync();

            return appointmentBlock;
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new AppDbContext(options, CreateConfiguration(), tenantContext);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
        }

        private sealed record SeedData(
            Tenant Tenant,
            User User,
            Branch PrimaryBranch,
            Branch SecondaryBranch);
    }
}
