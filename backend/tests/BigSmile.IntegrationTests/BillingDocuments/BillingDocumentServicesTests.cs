using BigSmile.Application.Features.BillingDocuments.Commands;
using BigSmile.Application.Features.BillingDocuments.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.BillingDocuments
{
    public class BillingDocumentServicesTests
    {
        [Fact]
        public async Task CreateAndGet_SucceedWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var treatmentQuote = await SeedAcceptedTreatmentQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var created = await commandService.CreateAsync(patient.Id);

            Assert.Equal(patient.Id, created.PatientId);
            Assert.Equal(treatmentQuote.Id, created.TreatmentQuoteId);
            Assert.Equal("Draft", created.Status);
            Assert.Equal("MXN", created.CurrencyCode);
            Assert.Equal(350m, created.TotalAmount);
            var item = Assert.Single(created.Items);
            Assert.Equal("Exam", item.Title);
            Assert.Equal(350m, item.UnitPrice);
            Assert.Equal(350m, item.LineTotal);
            Assert.Equal(actorUserId, item.CreatedByUserId);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal(created.BillingDocumentId, loaded!.BillingDocumentId);
            Assert.Equal("Draft", loaded.Status);
            Assert.Single(loaded.Items);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_WhenBillingDocumentDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedAcceptedTreatmentQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var billingDocument = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.Null(billingDocument);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentQuoteDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("accepted treatment quote is required", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentQuoteIsNotAccepted()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedDraftTreatmentQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("to be Accepted", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenTreatmentQuoteDoesNotContainItems()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var treatmentQuote = await SeedAcceptedTreatmentQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            await RemoveAllTreatmentQuoteItemsAsync(databaseName, treatmentQuote.Id);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("contain at least one item", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenBillingDocumentAlreadyExistsForTreatmentQuote()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedAcceptedTreatmentQuoteWithBillingDocumentAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ChangeStatusAsync_UpdatesStatusWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedAcceptedTreatmentQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var created = await commandService.CreateAsync(patient.Id);

            var issued = await commandService.ChangeStatusAsync(
                patient.Id,
                new ChangeBillingDocumentStatusCommand("Issued"));

            Assert.NotNull(issued);
            Assert.Equal(created.BillingDocumentId, issued!.BillingDocumentId);
            Assert.Equal("Issued", issued.Status);
            Assert.Equal(actorUserId, issued.LastUpdatedByUserId);
            Assert.Equal(actorUserId, issued.IssuedByUserId);
            Assert.NotNull(issued.IssuedAtUtc);
        }

        [Fact]
        public async Task ChangeStatusAsync_BlocksFurtherStatusChanges_WhenBillingDocumentIsAlreadyIssued()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedAcceptedTreatmentQuoteWithBillingDocumentAsync(databaseName, tenantA.Id, patient.Id, actorUserId, issueBillingDocument: true);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.ChangeStatusAsync(
                patient.Id,
                new ChangeBillingDocumentStatusCommand("Issued")));

            Assert.Contains("read-only", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SnapshotOnly_CreateAsync_DoesNotResync_WhenQuoteChangesLater()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var treatmentQuote = await SeedAcceptedTreatmentQuoteAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);
            var created = await commandService.CreateAsync(patient.Id);

            await ForceQuoteItemUnitPriceAsync(databaseName, treatmentQuote.Items.Single().Id, 425m);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal(350m, loaded!.Items.Single().UnitPrice);
            Assert.Equal(350m, loaded.Items.Single().LineTotal);
            Assert.Equal(350m, loaded.TotalAmount);
            Assert.Equal(created.BillingDocumentId, loaded.BillingDocumentId);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_ForCrossTenantAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedAcceptedTreatmentQuoteWithBillingDocumentAsync(databaseName, tenantA.Id, patientA.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var billingDocument = await queryService.GetByPatientIdAsync(patientA.Id);

            Assert.Null(billingDocument);
        }

        [Fact]
        public async Task CreateAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedAcceptedTreatmentQuoteAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(foreignPatient.Id));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ChangeStatusAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedAcceptedTreatmentQuoteWithBillingDocumentAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.ChangeStatusAsync(
                foreignPatient.Id,
                new ChangeBillingDocumentStatusCommand("Issued")));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PlatformScopedBillingAccessWithoutTenantContext_IsBlocked()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedAcceptedTreatmentQuoteWithBillingDocumentAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Platform, isAuthenticated: true);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.GetByPatientIdAsync(patient.Id));

            Assert.Contains("resolved tenant context", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static BillingDocumentCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new BillingDocumentCommandService(
                new EfBillingDocumentRepository(context),
                new EfTreatmentQuoteRepository(context),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static BillingDocumentQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new BillingDocumentQueryService(
                new EfBillingDocumentRepository(context),
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

        private static async Task<TreatmentQuote> SeedDraftTreatmentQuoteAsync(
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

        private static async Task<TreatmentQuote> SeedAcceptedTreatmentQuoteAsync(
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

            var quoteItemId = treatmentQuote.Items.Single().Id;
            treatmentQuote.UpdateItemUnitPrice(quoteItemId, 350m, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, actorUserId);

            context.TreatmentPlans.Add(treatmentPlan);
            context.TreatmentQuotes.Add(treatmentQuote);
            await context.SaveChangesAsync();

            return treatmentQuote;
        }

        private static async Task SeedAcceptedTreatmentQuoteWithBillingDocumentAsync(
            string databaseName,
            Guid tenantId,
            Guid patientId,
            Guid actorUserId,
            bool issueBillingDocument = false)
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

            var quoteItemId = treatmentQuote.Items.Single().Id;
            treatmentQuote.UpdateItemUnitPrice(quoteItemId, 350m, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, actorUserId);
            treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, actorUserId);

            var billingDocument = new BillingDocument(
                tenantId,
                patientId,
                treatmentQuote.Id,
                treatmentQuote.CurrencyCode,
                treatmentQuote.Items,
                actorUserId);

            if (issueBillingDocument)
            {
                billingDocument.ChangeStatus(BillingDocumentStatus.Issued, actorUserId);
            }

            context.TreatmentPlans.Add(treatmentPlan);
            context.TreatmentQuotes.Add(treatmentQuote);
            context.BillingDocuments.Add(billingDocument);
            await context.SaveChangesAsync();
        }

        private static async Task RemoveAllTreatmentQuoteItemsAsync(string databaseName, Guid treatmentQuoteId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var items = await context.TreatmentQuoteItems
                .Where(item => item.TreatmentQuoteId == treatmentQuoteId)
                .ToListAsync();

            context.TreatmentQuoteItems.RemoveRange(items);
            await context.SaveChangesAsync();
        }

        private static async Task ForceQuoteItemUnitPriceAsync(string databaseName, Guid quoteItemId, decimal unitPrice)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var quoteItem = await context.TreatmentQuoteItems.SingleAsync(item => item.Id == quoteItemId);
            context.Entry(quoteItem).Property(nameof(TreatmentQuoteItem.UnitPrice)).CurrentValue = unitPrice;
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
