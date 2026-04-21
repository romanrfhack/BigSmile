using BigSmile.Application.Features.Odontograms.Commands;
using BigSmile.Application.Features.Odontograms.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.Odontogram
{
    public class OdontogramServicesTests
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
            Assert.Equal(32, created.Teeth.Count);
            Assert.Empty(created.FindingsHistory);
            Assert.All(created.Teeth, tooth => Assert.Equal("Unknown", tooth.Status));
            Assert.All(created.Teeth, tooth =>
            {
                Assert.Equal(5, tooth.Surfaces.Count);
                Assert.All(tooth.Surfaces, surface => Assert.Equal("Unknown", surface.Status));
            });

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal(32, loaded!.Teeth.Count);
            Assert.Empty(loaded.FindingsHistory);
            Assert.All(loaded.Teeth, tooth => Assert.Equal(5, tooth.Surfaces.Count));
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_WhenOdontogramDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var odontogram = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.Null(odontogram);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenOdontogramAlreadyExistsForPatient()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(patient.Id));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateToothStatusAsync_SucceedsWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.UpdateToothStatusAsync(
                patient.Id,
                new UpdateOdontogramToothStatusCommand("11", "Caries"));

            Assert.NotNull(updated);
            var tooth = Assert.Single(updated!.Teeth, entry => entry.ToothCode == "11");
            Assert.Equal("Caries", tooth.Status);
            Assert.Equal(actorUserId, tooth.UpdatedByUserId);
            Assert.Equal(actorUserId, updated.LastUpdatedByUserId);
        }

        [Fact]
        public async Task UpdateSurfaceStatusAsync_SucceedsWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.UpdateSurfaceStatusAsync(
                patient.Id,
                new UpdateOdontogramSurfaceStatusCommand("11", "O", "Caries"));

            Assert.NotNull(updated);
            var tooth = Assert.Single(updated!.Teeth, entry => entry.ToothCode == "11");
            var surface = Assert.Single(tooth.Surfaces, entry => entry.SurfaceCode == "O");
            Assert.Equal("Caries", surface.Status);
            Assert.Equal(actorUserId, surface.UpdatedByUserId);
            Assert.Equal(actorUserId, updated.LastUpdatedByUserId);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_EnrichesReadModelWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var updated = await commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Caries"));

            Assert.NotNull(updated);
            var tooth = Assert.Single(updated!.Teeth, entry => entry.ToothCode == "11");
            var surface = Assert.Single(tooth.Surfaces, entry => entry.SurfaceCode == "O");
            var finding = Assert.Single(surface.Findings);
            Assert.Equal("Caries", finding.FindingType);
            Assert.Equal(actorUserId, finding.CreatedByUserId);
            var historyEntry = Assert.Single(updated.FindingsHistory);
            Assert.Equal("FindingAdded", historyEntry.EntryType);
            Assert.Equal("11", historyEntry.ToothCode);
            Assert.Equal("O", historyEntry.SurfaceCode);
            Assert.Equal("Caries", historyEntry.FindingType);
            Assert.Equal(actorUserId, historyEntry.ChangedByUserId);
            Assert.Equal("Finding added", historyEntry.Summary);
            Assert.Equal(finding.FindingId, historyEntry.ReferenceFindingId);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);
            var loadedTooth = Assert.Single(loaded!.Teeth, entry => entry.ToothCode == "11");
            var loadedSurface = Assert.Single(loadedTooth.Surfaces, entry => entry.SurfaceCode == "O");
            Assert.Single(loadedSurface.Findings);
            var loadedHistory = Assert.Single(loaded.FindingsHistory);
            Assert.Equal("FindingAdded", loadedHistory.EntryType);
            Assert.Equal("11", loadedHistory.ToothCode);
            Assert.Equal("O", loadedHistory.SurfaceCode);
        }

        [Fact]
        public async Task RemoveSurfaceFindingAsync_RemovesRequestedFinding()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var created = await commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Caries"));
            var findingId = created!.Teeth
                .Single(entry => entry.ToothCode == "11")
                .Surfaces.Single(entry => entry.SurfaceCode == "O")
                .Findings.Single()
                .FindingId;

            var updated = await commandService.RemoveSurfaceFindingAsync(patient.Id, "11", "O", findingId);

            Assert.NotNull(updated);
            var tooth = Assert.Single(updated!.Teeth, entry => entry.ToothCode == "11");
            var surface = Assert.Single(tooth.Surfaces, entry => entry.SurfaceCode == "O");
            Assert.Empty(surface.Findings);
            Assert.Equal(
                new[] { "FindingRemoved", "FindingAdded" },
                updated.FindingsHistory.Select(entry => entry.EntryType).ToArray());
            var removedEntry = updated.FindingsHistory[0];
            Assert.Equal("11", removedEntry.ToothCode);
            Assert.Equal("O", removedEntry.SurfaceCode);
            Assert.Equal("Caries", removedEntry.FindingType);
            Assert.Equal(actorUserId, removedEntry.ChangedByUserId);
            Assert.Equal("Finding removed", removedEntry.Summary);
            Assert.Equal(findingId, removedEntry.ReferenceFindingId);
            Assert.Equal(actorUserId, updated.LastUpdatedByUserId);
        }

        [Fact]
        public async Task UpdateToothStatusAsync_ReturnsNull_WhenOdontogramDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.UpdateToothStatusAsync(
                patient.Id,
                new UpdateOdontogramToothStatusCommand("11", "Healthy"));

            Assert.Null(updated);
        }

        [Fact]
        public async Task UpdateSurfaceStatusAsync_ReturnsNull_WhenOdontogramDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.UpdateSurfaceStatusAsync(
                patient.Id,
                new UpdateOdontogramSurfaceStatusCommand("11", "O", "Healthy"));

            Assert.Null(updated);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_ReturnsNull_WhenOdontogramDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Caries"));

            Assert.Null(updated);
        }

        [Fact]
        public async Task UpdateToothStatusAsync_Fails_ForInvalidToothCode()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.UpdateToothStatusAsync(
                patient.Id,
                new UpdateOdontogramToothStatusCommand("55", "Healthy")));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateSurfaceStatusAsync_Fails_ForInvalidToothCode()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.UpdateSurfaceStatusAsync(
                patient.Id,
                new UpdateOdontogramSurfaceStatusCommand("55", "O", "Healthy")));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateSurfaceStatusAsync_Fails_ForInvalidSurfaceCode()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.UpdateSurfaceStatusAsync(
                patient.Id,
                new UpdateOdontogramSurfaceStatusCommand("11", "X", "Healthy")));

            Assert.Contains("Surface code must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_Fails_ForInvalidToothCode()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("55", "O", "Caries")));

            Assert.Contains("FDI permanent adult numbering", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_Fails_ForInvalidSurfaceCode()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "X", "Caries")));

            Assert.Contains("Surface code must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_Fails_ForInvalidFindingType()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Fracture")));

            Assert.Contains("Finding type must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_Fails_ForExactDuplicateFinding()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            await commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Caries"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddSurfaceFindingAsync(
                patient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Caries")));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_ForCrossTenantAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedOdontogramAsync(databaseName, tenantA.Id, patientA.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var odontogram = await queryService.GetByPatientIdAsync(patientA.Id);

            Assert.Null(odontogram);
        }

        [Fact]
        public async Task UpdateToothStatusAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedOdontogramAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.UpdateToothStatusAsync(
                foreignPatient.Id,
                new UpdateOdontogramToothStatusCommand("11", "Healthy")));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task UpdateSurfaceStatusAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedOdontogramAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.UpdateSurfaceStatusAsync(
                foreignPatient.Id,
                new UpdateOdontogramSurfaceStatusCommand("11", "O", "Healthy")));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddSurfaceFindingAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedOdontogramAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddSurfaceFindingAsync(
                foreignPatient.Id,
                new AddOdontogramSurfaceFindingCommand("11", "O", "Caries")));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PlatformScopedOdontogramAccessWithoutTenantContext_IsBlocked()
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

        private static OdontogramCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new OdontogramCommandService(
                new EfOdontogramRepository(context),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static OdontogramQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new OdontogramQueryService(
                new EfOdontogramRepository(context),
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

        private static async Task<Domain.Entities.Odontogram> SeedOdontogramAsync(string databaseName, Guid tenantId, Guid patientId, Guid actorUserId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var odontogram = new Domain.Entities.Odontogram(tenantId, patientId, actorUserId);

            context.Odontograms.Add(odontogram);
            await context.SaveChangesAsync();

            return odontogram;
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
    }
}
