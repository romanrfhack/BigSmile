using BigSmile.Application.Features.TreatmentPlans.Commands;
using BigSmile.Application.Features.TreatmentPlans.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.TreatmentPlans
{
    public class TreatmentPlanServicesTests
    {
        [Fact]
        public async Task CreateAndGet_SucceedWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var created = await commandService.CreateAsync(patient.Id);

            Assert.Equal(patient.Id, created.PatientId);
            Assert.Equal("Draft", created.Status);
            Assert.Empty(created.Items);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal("Draft", loaded!.Status);
            Assert.Empty(loaded.Items);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_WhenTreatmentPlanDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var treatmentPlan = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.Null(treatmentPlan);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentPlanAlreadyExistsForPatient()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddItemAsync_SucceedsWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.AddItemAsync(
                patient.Id,
                new AddTreatmentPlanItemCommand(
                    "Composite restoration",
                    "Restorative",
                    2,
                    "Two-surface restoration.",
                    "11",
                    "O"));

            Assert.NotNull(updated);
            var item = Assert.Single(updated!.Items);
            Assert.Equal("Composite restoration", item.Title);
            Assert.Equal("11", item.ToothCode);
            Assert.Equal("O", item.SurfaceCode);
            Assert.Equal(actorUserId, item.CreatedByUserId);
            Assert.Equal(actorUserId, updated.LastUpdatedByUserId);
        }

        [Fact]
        public async Task AddItemAsync_ReturnsNull_WhenTreatmentPlanDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var updated = await commandService.AddItemAsync(
                patient.Id,
                new AddTreatmentPlanItemCommand("Exam", "Diagnostics", 1, null, null, null));

            Assert.Null(updated);
            Assert.Null(await queryService.GetByPatientIdAsync(patient.Id));
        }

        [Fact]
        public async Task RemoveItemAsync_RemovesRequestedItem()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var writeContext = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(writeContext, tenantContext);
            var withItems = await commandService.AddItemAsync(
                patient.Id,
                new AddTreatmentPlanItemCommand("Exam", "Diagnostics", 1, null, null, null));
            withItems = await commandService.AddItemAsync(
                patient.Id,
                new AddTreatmentPlanItemCommand("Sealant", "Preventive", 1, null, "16", "O"));

            var itemId = withItems!.Items.Single(item => item.Title == "Sealant").ItemId;

            var updated = await commandService.RemoveItemAsync(patient.Id, itemId);

            Assert.NotNull(updated);
            Assert.Single(updated!.Items);
            Assert.Equal("Exam", updated.Items[0].Title);
        }

        [Fact]
        public async Task ChangeStatusAsync_UpdatesStatusWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var proposed = await commandService.ChangeStatusAsync(
                patient.Id,
                new ChangeTreatmentPlanStatusCommand("Proposed"));

            Assert.NotNull(proposed);
            Assert.Equal("Proposed", proposed!.Status);
            Assert.Equal(actorUserId, proposed.LastUpdatedByUserId);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_ForCrossTenantAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patientA.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var treatmentPlan = await queryService.GetByPatientIdAsync(patientA.Id);

            Assert.Null(treatmentPlan);
        }

        [Fact]
        public async Task AddItemAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedTreatmentPlanAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddItemAsync(
                foreignPatient.Id,
                new AddTreatmentPlanItemCommand("Exam", "Diagnostics", 1, null, null, null)));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ChangeStatusAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedTreatmentPlanAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.ChangeStatusAsync(
                foreignPatient.Id,
                new ChangeTreatmentPlanStatusCommand("Proposed")));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PlatformScopedTreatmentPlanAccessWithoutTenantContext_IsBlocked()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Platform, isAuthenticated: true);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.GetByPatientIdAsync(patient.Id));

            Assert.Contains("resolved tenant context", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static TreatmentPlanCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new TreatmentPlanCommandService(
                new EfTreatmentPlanRepository(context),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static TreatmentPlanQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new TreatmentPlanQueryService(
                new EfTreatmentPlanRepository(context),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(userId.ToString(), AccessScope.Tenant, isAuthenticated: true, tenantId.ToString());
            return tenantContext;
        }

        private static async Task<(Tenant TenantA, Tenant TenantB)> SeedTenantsAsync(string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            tenantA.AddBranch("Branch A");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            tenantB.AddBranch("Branch B");

            context.Tenants.AddRange(tenantA, tenantB);
            await context.SaveChangesAsync();

            return (tenantA, tenantB);
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

        private static async Task SeedTreatmentPlanAsync(string databaseName, Guid tenantId, Guid patientId, Guid actorUserId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            context.TreatmentPlans.Add(new TreatmentPlan(tenantId, patientId, actorUserId));
            await context.SaveChangesAsync();
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var configuration = new ConfigurationBuilder().Build();
            return new AppDbContext(options, configuration, tenantContext);
        }
    }
}
