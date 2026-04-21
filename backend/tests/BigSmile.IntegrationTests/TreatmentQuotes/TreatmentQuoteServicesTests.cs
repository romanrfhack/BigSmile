using BigSmile.Application.Features.TreatmentQuotes.Commands;
using BigSmile.Application.Features.TreatmentQuotes.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.TreatmentQuotes
{
    public class TreatmentQuoteServicesTests
    {
        [Fact]
        public async Task CreateAndGet_SucceedWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var treatmentPlan = await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId, includeItem: true);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var created = await commandService.CreateAsync(patient.Id);

            Assert.Equal(patient.Id, created.PatientId);
            Assert.Equal(treatmentPlan.Id, created.TreatmentPlanId);
            Assert.Equal("Draft", created.Status);
            Assert.Equal("MXN", created.CurrencyCode);
            Assert.Equal(0m, created.Total);
            var item = Assert.Single(created.Items);
            Assert.Equal("Exam", item.Title);
            Assert.Equal(0m, item.UnitPrice);
            Assert.Equal(actorUserId, item.CreatedByUserId);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal(created.TreatmentQuoteId, loaded!.TreatmentQuoteId);
            Assert.Equal("Draft", loaded.Status);
            Assert.Single(loaded.Items);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_WhenTreatmentQuoteDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId, includeItem: true);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var treatmentQuote = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.Null(treatmentQuote);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentPlanDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("treatment plan is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentPlanDoesNotContainItems()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanAsync(databaseName, tenantA.Id, patient.Id, actorUserId, includeItem: false);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("contain at least one item", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentQuoteAlreadyExistsForTreatmentPlan()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateItemUnitPriceAsync_SucceedsWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var treatmentQuote = await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var itemId = treatmentQuote.Items.Single().Id;

            var updated = await commandService.UpdateItemUnitPriceAsync(
                patient.Id,
                itemId,
                new UpdateTreatmentQuoteItemPriceCommand(499.99m));

            Assert.NotNull(updated);
            Assert.Equal(499.99m, updated!.Items.Single().UnitPrice);
            Assert.Equal(499.99m, updated.Items.Single().LineTotal);
            Assert.Equal(499.99m, updated.Total);
            Assert.Equal(actorUserId, updated.LastUpdatedByUserId);
        }

        [Fact]
        public async Task ChangeStatusAsync_UpdatesStatusWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var treatmentQuote = await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var quoteItemId = treatmentQuote.Items.Single().Id;
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            await commandService.UpdateItemUnitPriceAsync(
                patient.Id,
                quoteItemId,
                new UpdateTreatmentQuoteItemPriceCommand(350m));

            var proposed = await commandService.ChangeStatusAsync(
                patient.Id,
                new ChangeTreatmentQuoteStatusCommand("Proposed"));

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
            await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantA.Id, patientA.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var treatmentQuote = await queryService.GetByPatientIdAsync(patientA.Id);

            Assert.Null(treatmentQuote);
        }

        [Fact]
        public async Task UpdateItemUnitPriceAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            var treatmentQuote = await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.UpdateItemUnitPriceAsync(
                foreignPatient.Id,
                treatmentQuote.Items.Single().Id,
                new UpdateTreatmentQuoteItemPriceCommand(250m)));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ChangeStatusAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.ChangeStatusAsync(
                foreignPatient.Id,
                new ChangeTreatmentQuoteStatusCommand("Proposed")));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PlatformScopedTreatmentQuoteAccessWithoutTenantContext_IsBlocked()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedTreatmentPlanWithQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Platform, isAuthenticated: true);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.GetByPatientIdAsync(patient.Id));

            Assert.Contains("resolved tenant context", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static TreatmentQuoteCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new TreatmentQuoteCommandService(
                new EfTreatmentQuoteRepository(context),
                new EfTreatmentPlanRepository(context),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static TreatmentQuoteQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new TreatmentQuoteQueryService(
                new EfTreatmentQuoteRepository(context),
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

        private static async Task<TreatmentPlan> SeedTreatmentPlanAsync(
            string databaseName,
            Guid tenantId,
            Guid patientId,
            Guid actorUserId,
            bool includeItem)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var treatmentPlan = new TreatmentPlan(tenantId, patientId, actorUserId);
            if (includeItem)
            {
                treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, actorUserId);
            }

            context.TreatmentPlans.Add(treatmentPlan);
            await context.SaveChangesAsync();

            return treatmentPlan;
        }

        private static async Task<TreatmentQuote> SeedTreatmentPlanWithQuoteAsync(
            string databaseName,
            Guid tenantId,
            Guid patientId,
            Guid actorUserId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var treatmentPlan = new TreatmentPlan(tenantId, patientId, actorUserId);
            treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, actorUserId);

            var treatmentQuote = new TreatmentQuote(
                tenantId,
                patientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            context.TreatmentPlans.Add(treatmentPlan);
            context.TreatmentQuotes.Add(treatmentQuote);
            await context.SaveChangesAsync();

            return treatmentQuote;
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
