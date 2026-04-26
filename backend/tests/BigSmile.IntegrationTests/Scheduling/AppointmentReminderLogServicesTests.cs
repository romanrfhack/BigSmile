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
    public class AppointmentReminderLogServicesTests
    {
        [Fact]
        public async Task AddAsync_CreatesManualEntryInsideTenantAndBranchScopeWithoutChangingAppointmentStatuses()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0),
                confirmByUserId: seedData.User.Id);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateReminderLogCommandService(context, tenantContext);

            var entry = await commandService.AddAsync(
                appointment.Id,
                new AddAppointmentReminderLogEntryCommand(
                    "Phone",
                    "Reached",
                    " Confirmed by phone. "));

            Assert.NotNull(entry);
            Assert.Equal(appointment.Id, entry!.AppointmentId);
            Assert.Equal("Phone", entry.Channel);
            Assert.Equal("Reached", entry.Outcome);
            Assert.Equal("Confirmed by phone.", entry.Notes);
            Assert.Equal(seedData.User.Id, entry.CreatedByUserId);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedEntry = await verificationContext.AppointmentReminderLogEntries.SingleAsync();
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(seedData.Tenant.Id, storedEntry.TenantId);
            Assert.Equal(appointment.Id, storedEntry.AppointmentId);
            Assert.Equal(AppointmentStatus.Scheduled, storedAppointment.Status);
            Assert.Equal(AppointmentConfirmationStatus.Confirmed, storedAppointment.ConfirmationStatus);
            Assert.NotNull(storedAppointment.ConfirmedAtUtc);
            Assert.Equal(seedData.User.Id, storedAppointment.ConfirmedByUserId);
        }

        [Fact]
        public async Task AddAsync_AllowsManualEntryForTerminalAppointmentStatus()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0),
                status: AppointmentStatus.NoShow);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateReminderLogCommandService(context, tenantContext);

            var entry = await commandService.AddAsync(
                appointment.Id,
                new AddAppointmentReminderLogEntryCommand("WhatsApp", "NoAnswer", null));

            Assert.NotNull(entry);
            Assert.Equal("WhatsApp", entry!.Channel);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedAppointment = await verificationContext.Appointments.SingleAsync();

            Assert.Equal(AppointmentStatus.NoShow, storedAppointment.Status);
            Assert.Equal(AppointmentConfirmationStatus.Pending, storedAppointment.ConfirmationStatus);
        }

        [Fact]
        public async Task ListAsync_ReturnsManualEntriesNewestFirst()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0));
            await SeedReminderLogEntryAsync(
                databaseName,
                seedData.Tenant.Id,
                appointment.Id,
                AppointmentReminderChannel.Email,
                AppointmentReminderOutcome.LeftMessage,
                new DateTime(2026, 4, 24, 14, 0, 0));
            await SeedReminderLogEntryAsync(
                databaseName,
                seedData.Tenant.Id,
                appointment.Id,
                AppointmentReminderChannel.Phone,
                AppointmentReminderOutcome.Reached,
                new DateTime(2026, 4, 25, 8, 0, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateReminderLogQueryService(context, tenantContext);

            var entries = await queryService.ListAsync(appointment.Id);

            Assert.NotNull(entries);
            Assert.Equal(new[] { "Phone", "Email" }, entries!.Select(entry => entry.Channel));
            Assert.Equal(new[] { "Reached", "LeftMessage" }, entries.Select(entry => entry.Outcome));
        }

        [Fact]
        public async Task AddAsync_ReturnsNullForAppointmentOutsideCurrentTenant()
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
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0));
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateReminderLogCommandService(context, tenantContext);

            var entry = await commandService.AddAsync(
                foreignAppointment.Id,
                new AddAppointmentReminderLogEntryCommand("Phone", "Reached", null));

            Assert.Null(entry);
        }

        [Fact]
        public async Task ListAsync_ReturnsNullForAppointmentOutsideCurrentTenant()
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
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0));
            var tenantContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateReminderLogQueryService(context, tenantContext);

            var entries = await queryService.ListAsync(foreignAppointment.Id);

            Assert.Null(entries);
        }

        [Fact]
        public async Task AddAsync_BlocksBranchOutsideAssignedScope_ForTenantUser()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.SecondaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 25, 11, 0, 0),
                new DateTime(2026, 4, 25, 11, 30, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateReminderLogCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddAsync(
                appointment.Id,
                new AddAppointmentReminderLogEntryCommand("Phone", "Reached", null)));

            Assert.Contains("branch is not accessible", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static AppointmentReminderLogCommandService CreateReminderLogCommandService(
            AppDbContext context,
            TenantContext tenantContext)
        {
            return new AppointmentReminderLogCommandService(
                new EfAppointmentRepository(context),
                new EfAppointmentReminderLogRepository(context),
                new BranchAccessService(
                    new EfBranchRepository(context),
                    new EfUserTenantMembershipRepository(context),
                    new RolePermissionCatalog(),
                    tenantContext),
                tenantContext);
        }

        private static AppointmentReminderLogQueryService CreateReminderLogQueryService(
            AppDbContext context,
            TenantContext tenantContext)
        {
            return new AppointmentReminderLogQueryService(
                new EfAppointmentRepository(context),
                new EfAppointmentReminderLogRepository(context),
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
            AppointmentStatus status = AppointmentStatus.Scheduled,
            Guid? confirmByUserId = null)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointment = new Appointment(tenantId, branchId, patientId, startsAt, endsAt, "Follow-up");

            switch (status)
            {
                case AppointmentStatus.Cancelled:
                    appointment.Cancel("Cancelled during test setup.");
                    break;
                case AppointmentStatus.Attended:
                    appointment.MarkAttended();
                    break;
                case AppointmentStatus.NoShow:
                    appointment.MarkNoShow();
                    break;
            }

            if (confirmByUserId.HasValue)
            {
                appointment.Confirm(confirmByUserId.Value);
            }

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            return appointment;
        }

        private static async Task SeedReminderLogEntryAsync(
            string databaseName,
            Guid tenantId,
            Guid appointmentId,
            AppointmentReminderChannel channel,
            AppointmentReminderOutcome outcome,
            DateTime createdAtUtc)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var entry = new AppointmentReminderLogEntry(
                tenantId,
                appointmentId,
                channel,
                outcome,
                null,
                Guid.NewGuid(),
                createdAtUtc);

            context.AppointmentReminderLogEntries.Add(entry);
            await context.SaveChangesAsync();
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
