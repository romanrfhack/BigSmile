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
    public class AppointmentManualReminderServicesTests
    {
        [Fact]
        public async Task ConfigureCompleteAndClearAsync_WorkInsideTenantAndBranchScopeWithoutChangingStatusesOrCreatingReminderLog()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 27, 9, 0, 0),
                new DateTime(2026, 4, 27, 9, 30, 0),
                confirmByUserId: seedData.User.Id);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var dueAtUtc = DateTime.UtcNow.AddHours(2);

            var configured = await commandService.ConfigureManualReminderAsync(
                appointment.Id,
                new ConfigureAppointmentManualReminderCommand(true, "Phone", dueAtUtc));

            Assert.NotNull(configured);
            Assert.True(configured!.ReminderRequired);
            Assert.Equal("Phone", configured.ReminderChannel);
            Assert.Equal(dueAtUtc, configured.ReminderDueAtUtc);
            Assert.Null(configured.ReminderCompletedAtUtc);
            Assert.Null(configured.ReminderCompletedByUserId);
            Assert.Equal("Scheduled", configured.Status);
            Assert.Equal("Confirmed", configured.ConfirmationStatus);

            var completed = await commandService.CompleteManualReminderAsync(appointment.Id);

            Assert.NotNull(completed);
            Assert.True(completed!.ReminderRequired);
            Assert.NotNull(completed.ReminderCompletedAtUtc);
            Assert.Equal(seedData.User.Id, completed.ReminderCompletedByUserId);
            Assert.Equal("Scheduled", completed.Status);
            Assert.Equal("Confirmed", completed.ConfirmationStatus);

            var cleared = await commandService.ConfigureManualReminderAsync(
                appointment.Id,
                new ConfigureAppointmentManualReminderCommand(false, null, null));

            Assert.NotNull(cleared);
            Assert.False(cleared!.ReminderRequired);
            Assert.Null(cleared.ReminderChannel);
            Assert.Null(cleared.ReminderDueAtUtc);
            Assert.Null(cleared.ReminderCompletedAtUtc);
            Assert.Null(cleared.ReminderCompletedByUserId);
            Assert.Equal("Scheduled", cleared.Status);
            Assert.Equal("Confirmed", cleared.ConfirmationStatus);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(seedData.Tenant.Id, storedAppointment.TenantId);
            Assert.Equal(seedData.PrimaryBranch.Id, storedAppointment.BranchId);
            Assert.Equal(patient.Id, storedAppointment.PatientId);
            Assert.Equal(AppointmentStatus.Scheduled, storedAppointment.Status);
            Assert.Equal(AppointmentConfirmationStatus.Confirmed, storedAppointment.ConfirmationStatus);
            Assert.Empty(await verificationContext.AppointmentReminderLogEntries.ToListAsync());
        }

        [Fact]
        public async Task ListManualRemindersAsync_ReturnsPendingAndDueOrderedByDueAtAndExcludesCompletedByDefault()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var patientB = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Bruno", "Garcia");
            var patientC = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Carla", "Mendez");
            var dueAppointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patientA.Id,
                DateTime.UtcNow.AddHours(3),
                DateTime.UtcNow.AddHours(4));
            var pendingAppointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patientB.Id,
                DateTime.UtcNow.AddHours(5),
                DateTime.UtcNow.AddHours(6));
            var completedAppointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patientC.Id,
                DateTime.UtcNow.AddHours(7),
                DateTime.UtcNow.AddHours(8));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var commandContext = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(commandContext, tenantContext);
            await commandService.ConfigureManualReminderAsync(
                pendingAppointment.Id,
                new ConfigureAppointmentManualReminderCommand(true, "Email", DateTime.UtcNow.AddHours(3)));
            await commandService.ConfigureManualReminderAsync(
                dueAppointment.Id,
                new ConfigureAppointmentManualReminderCommand(true, "Phone", DateTime.UtcNow.AddHours(-2)));
            await commandService.ConfigureManualReminderAsync(
                completedAppointment.Id,
                new ConfigureAppointmentManualReminderCommand(true, "WhatsApp", DateTime.UtcNow.AddHours(-1)));
            await commandService.CompleteManualReminderAsync(completedAppointment.Id);

            await using var queryContext = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(queryContext, tenantContext);

            var defaultReminders = await queryService.ListManualRemindersAsync(seedData.PrimaryBranch.Id);

            Assert.Equal(new[] { dueAppointment.Id, pendingAppointment.Id }, defaultReminders.Select(item => item.AppointmentId));
            Assert.Equal(new[] { "Due", "Pending" }, defaultReminders.Select(item => item.ReminderState));
            Assert.Equal(new[] { "Phone", "Email" }, defaultReminders.Select(item => item.ReminderChannel));

            var withCompleted = await queryService.ListManualRemindersAsync(seedData.PrimaryBranch.Id, includeCompleted: true);

            Assert.Contains(withCompleted, item => item.AppointmentId == completedAppointment.Id && item.ReminderState == "Completed");
        }

        [Fact]
        public async Task ConfigureManualReminderAsync_ReturnsNullForAppointmentOutsideCurrentTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var patientB = await SeedPatientAsync(databaseName, tenantBSeed.Tenant.Id, "Bruno", "Garcia");
            var foreignAppointment = await SeedAppointmentAsync(
                databaseName,
                tenantBSeed.Tenant.Id,
                tenantBSeed.PrimaryBranch.Id,
                patientB.Id,
                DateTime.UtcNow.AddHours(2),
                DateTime.UtcNow.AddHours(3));
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var configured = await commandService.ConfigureManualReminderAsync(
                foreignAppointment.Id,
                new ConfigureAppointmentManualReminderCommand(true, "Phone", DateTime.UtcNow.AddHours(1)));

            Assert.Null(configured);
        }

        [Fact]
        public async Task ListManualRemindersAsync_BlocksForeignTenantBranch()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                queryService.ListManualRemindersAsync(tenantBSeed.PrimaryBranch.Id));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ConfigureManualReminderAsync_BlocksBranchOutsideAssignedScopeForTenantUser()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.SecondaryBranch.Id,
                patient.Id,
                DateTime.UtcNow.AddHours(2),
                DateTime.UtcNow.AddHours(3));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                commandService.ConfigureManualReminderAsync(
                    appointment.Id,
                    new ConfigureAppointmentManualReminderCommand(true, "Phone", DateTime.UtcNow.AddHours(1))));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static AppointmentCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new AppointmentCommandService(
                new EfAppointmentRepository(context),
                new EfAppointmentBlockRepository(context),
                new EfPatientRepository(context),
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

        private static async Task<Patient> SeedPatientAsync(string databaseName, Guid tenantId, string firstName, string lastName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var patient = new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14),
                "5551234567",
                $"{firstName.ToLowerInvariant()}@example.com");

            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            return patient;
        }

        private static async Task<Appointment> SeedAppointmentAsync(
            string databaseName,
            Guid tenantId,
            Guid branchId,
            Guid patientId,
            DateTime startsAt,
            DateTime endsAt,
            Guid? confirmByUserId = null)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointment = new Appointment(tenantId, branchId, patientId, startsAt, endsAt, "Follow-up");

            if (confirmByUserId.HasValue)
            {
                appointment.Confirm(confirmByUserId.Value);
            }

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            return appointment;
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
