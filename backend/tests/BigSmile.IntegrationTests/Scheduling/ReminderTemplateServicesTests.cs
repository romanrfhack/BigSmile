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
    public sealed class ReminderTemplateServicesTests
    {
        [Fact]
        public async Task CreateListUpdateAndDeactivateAsync_WorkInsideTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var second = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                " Later template ",
                " Hola {{patientName}}. "));
            var first = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                " Confirmacion ",
                " Recordatorio para {{appointmentDate}}. "));

            Assert.Equal("Later template", second.Name);
            Assert.Equal("Hola {{patientName}}.", second.Body);
            Assert.Equal("Confirmacion", first.Name);
            Assert.True(first.IsActive);
            Assert.Equal(seedData.User.Id, first.CreatedByUserId);

            var activeTemplates = await queryService.ListAsync();

            Assert.Equal(new[] { "Confirmacion", "Later template" }, activeTemplates.Select(template => template.Name));

            var updated = await commandService.UpdateAsync(
                first.Id,
                new UpdateReminderTemplateCommand("Confirmacion final", "Texto final"));

            Assert.NotNull(updated);
            Assert.Equal("Confirmacion final", updated!.Name);
            Assert.Equal("Texto final", updated.Body);
            Assert.Equal(seedData.User.Id, updated.UpdatedByUserId);

            var deactivated = await commandService.DeactivateAsync(first.Id);

            Assert.True(deactivated);

            var activeAfterDeactivate = await queryService.ListAsync();
            var includingInactive = await queryService.ListAsync(includeInactive: true);

            Assert.Equal(new[] { "Later template" }, activeAfterDeactivate.Select(template => template.Name));
            Assert.Contains(includingInactive, template =>
                template.Id == first.Id &&
                !template.IsActive &&
                template.DeactivatedByUserId == seedData.User.Id);
        }

        [Fact]
        public async Task UpdateAsync_BlocksInactiveTemplate()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName);
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var template = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                "Confirmacion",
                "Hola {{patientName}}."));
            await commandService.DeactivateAsync(template.Id);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                commandService.UpdateAsync(
                    template.Id,
                    new UpdateReminderTemplateCommand("Updated", "Updated body")));

            Assert.Contains("inactive", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PreviewAsync_RendersKnownPlaceholdersPreservesUnknownPlaceholdersAndDoesNotMutateState()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seedData = await SeedTenantWithUserAsync(databaseName, tenantName: "Clinica Centro");
            var patient = await SeedPatientAsync(databaseName, seedData.Tenant.Id, "Ana", "Lopez");
            var appointment = await SeedAppointmentAsync(
                databaseName,
                seedData.Tenant.Id,
                seedData.PrimaryBranch.Id,
                patient.Id,
                new DateTime(2026, 4, 28, 9, 15, 0),
                new DateTime(2026, 4, 28, 9, 45, 0));
            var tenantContext = CreateTenantContext(seedData.User.Id, seedData.Tenant.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);
            var template = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                "Confirmacion",
                "Hola {{patientName}}, cita {{appointmentDate}} {{appointmentTime}} en {{branchName}} de {{tenantName}}. {{doctorName}}"));

            var beforePreview = await context.Appointments
                .AsNoTracking()
                .SingleAsync(storedAppointment => storedAppointment.Id == appointment.Id);

            var preview = await queryService.PreviewAsync(template.Id, appointment.Id);

            Assert.NotNull(preview);
            Assert.Equal(template.Id, preview!.TemplateId);
            Assert.Equal(appointment.Id, preview.AppointmentId);
            Assert.Equal(
                "Hola Ana Lopez, cita 2026-04-28 09:15 en Clinica Centro Main de Clinica Centro. {{doctorName}}",
                preview.RenderedBody);
            Assert.Equal(new[] { "doctorName" }, preview.UnknownPlaceholders);

            var afterPreview = await context.Appointments
                .AsNoTracking()
                .SingleAsync(storedAppointment => storedAppointment.Id == appointment.Id);
            var storedTemplate = await context.ReminderTemplates
                .AsNoTracking()
                .SingleAsync(storedTemplate => storedTemplate.Id == template.Id);

            Assert.Equal(beforePreview.ConfirmationStatus, afterPreview.ConfirmationStatus);
            Assert.Equal(beforePreview.ReminderRequired, afterPreview.ReminderRequired);
            Assert.Equal(beforePreview.ReminderChannel, afterPreview.ReminderChannel);
            Assert.Equal(beforePreview.ReminderDueAtUtc, afterPreview.ReminderDueAtUtc);
            Assert.Equal(beforePreview.ReminderCompletedAtUtc, afterPreview.ReminderCompletedAtUtc);
            Assert.Null(storedTemplate.UpdatedAtUtc);
            Assert.Empty(await context.AppointmentReminderLogEntries.ToListAsync());
        }

        [Fact]
        public async Task PreviewAsync_ReturnsNullForInactiveTemplateForeignTemplateOrForeignAppointment()
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
            var tenantAContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);
            var tenantBContext = CreateTenantContext(tenantBSeed.User.Id, tenantBSeed.Tenant.Id);

            ReminderTemplate tenantBTemplate;
            await using (var tenantBWriteContext = CreateContext(databaseName, tenantBContext))
            {
                var tenantBCommandService = CreateCommandService(tenantBWriteContext, tenantBContext);
                var created = await tenantBCommandService.CreateAsync(new CreateReminderTemplateCommand(
                    "Tenant B template",
                    "Hola {{patientName}}."));
                tenantBTemplate = (await tenantBWriteContext.ReminderTemplates.SingleAsync(template => template.Id == created.Id))!;
            }

            await using var context = CreateContext(databaseName, tenantAContext);
            var commandService = CreateCommandService(context, tenantAContext);
            var queryService = CreateQueryService(context, tenantAContext);
            var tenantAPatient = await SeedPatientAsync(databaseName, tenantASeed.Tenant.Id, "Ana", "Lopez");
            var tenantAAppointment = await SeedAppointmentAsync(
                databaseName,
                tenantASeed.Tenant.Id,
                tenantASeed.PrimaryBranch.Id,
                tenantAPatient.Id,
                DateTime.UtcNow.AddHours(4),
                DateTime.UtcNow.AddHours(5));
            var tenantATemplate = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                "Tenant A template",
                "Hola {{patientName}}."));
            var activeTenantATemplate = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                "Tenant A active template",
                "Hola {{patientName}}."));
            await commandService.DeactivateAsync(tenantATemplate.Id);

            var inactivePreview = await queryService.PreviewAsync(tenantATemplate.Id, tenantAAppointment.Id);
            var foreignTemplatePreview = await queryService.PreviewAsync(tenantBTemplate.Id, tenantAAppointment.Id);
            var foreignAppointmentPreview = await queryService.PreviewAsync(activeTenantATemplate.Id, foreignAppointment.Id);

            Assert.Null(inactivePreview);
            Assert.Null(foreignTemplatePreview);
            Assert.Null(foreignAppointmentPreview);
        }

        [Fact]
        public async Task PreviewAsync_ReturnsNullWhenAppointmentBranchIsOutsideAssignedScopeForTenantUser()
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
            var queryService = CreateQueryService(context, tenantContext);
            var template = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                "Confirmacion",
                "Hola {{patientName}}."));

            var preview = await queryService.PreviewAsync(template.Id, appointment.Id);

            Assert.Null(preview);
        }

        [Fact]
        public async Task ListAsync_IsTenantScopedAndIncludeInactiveDoesNotExposeOtherTenants()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantASeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant A", tenantSubdomain: "tenant-a");
            var tenantBSeed = await SeedTenantWithUserAsync(databaseName, tenantName: "Tenant B", tenantSubdomain: "tenant-b");
            var tenantAContext = CreateTenantContext(tenantASeed.User.Id, tenantASeed.Tenant.Id);
            var tenantBContext = CreateTenantContext(tenantBSeed.User.Id, tenantBSeed.Tenant.Id);

            await using (var tenantBWriteContext = CreateContext(databaseName, tenantBContext))
            {
                var tenantBCommandService = CreateCommandService(tenantBWriteContext, tenantBContext);
                await tenantBCommandService.CreateAsync(new CreateReminderTemplateCommand(
                    "Tenant B template",
                    "Hola."));
            }

            await using var context = CreateContext(databaseName, tenantAContext);
            var commandService = CreateCommandService(context, tenantAContext);
            var queryService = CreateQueryService(context, tenantAContext);
            var tenantATemplate = await commandService.CreateAsync(new CreateReminderTemplateCommand(
                "Tenant A inactive",
                "Hola."));
            await commandService.DeactivateAsync(tenantATemplate.Id);

            var templates = await queryService.ListAsync(includeInactive: true);

            Assert.Single(templates);
            Assert.Equal("Tenant A inactive", templates[0].Name);
            Assert.False(templates[0].IsActive);
        }

        private static ReminderTemplateCommandService CreateCommandService(
            AppDbContext context,
            TenantContext tenantContext)
        {
            return new ReminderTemplateCommandService(
                new EfReminderTemplateRepository(context),
                tenantContext);
        }

        private static ReminderTemplateQueryService CreateQueryService(
            AppDbContext context,
            TenantContext tenantContext)
        {
            return new ReminderTemplateQueryService(
                new EfReminderTemplateRepository(context),
                new EfAppointmentRepository(context),
                new EfTenantRepository(context),
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
            string tenantName = "Clinica Centro",
            string tenantSubdomain = "clinica-centro")
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

        private static async Task<Patient> SeedPatientAsync(
            string databaseName,
            Guid tenantId,
            string firstName,
            string lastName)
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
            DateTime endsAt)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointment = new Appointment(tenantId, branchId, patientId, startsAt, endsAt, "Follow-up");

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
