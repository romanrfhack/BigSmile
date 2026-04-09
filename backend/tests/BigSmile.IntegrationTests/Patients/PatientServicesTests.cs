using BigSmile.Application.Features.Patients.Commands;
using BigSmile.Application.Features.Patients.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.Patients
{
    public class PatientServicesTests
    {
        [Fact]
        public async Task CreateAsync_CreatesPatientInsideResolvedTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = new PatientCommandService(new EfPatientRepository(context), tenantContext);

            var patient = await commandService.CreateAsync(
                new SavePatientCommand(
                    "Ana",
                    "Lopez",
                    new DateOnly(1991, 2, 14),
                    "5551234567",
                    "ana@example.com",
                    true,
                    "Maria Lopez",
                    "Mother",
                    "5559990000"));

            Assert.Equal("Ana Lopez", patient.FullName);
            Assert.True(patient.IsActive);

            await using var verificationContext = CreateContext(databaseName, new TenantContext());
            var storedPatient = await verificationContext.Patients.SingleAsync();

            Assert.Equal(tenantA.Id, storedPatient.TenantId);
            Assert.Equal("Ana", storedPatient.FirstName);
            Assert.Equal("Maria Lopez", storedPatient.ResponsiblePartyName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullForCrossTenantPatientAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez", "5551234567");
            await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia", "5557654321");

            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantB.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = new PatientQueryService(new EfPatientRepository(context), tenantContext);

            var result = await queryService.GetByIdAsync(patientA.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesPatientInsideTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez", "5551234567");
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = new PatientCommandService(new EfPatientRepository(context), tenantContext);

            var updated = await commandService.UpdateAsync(
                patient.Id,
                new SavePatientCommand(
                    "Ana Sofia",
                    "Lopez",
                    new DateOnly(1991, 2, 14),
                    "5551112222",
                    "ana.sofia@example.com",
                    false,
                    "Mario Lopez",
                    "Father",
                    "5554448888"));

            Assert.NotNull(updated);
            Assert.Equal("Ana Sofia Lopez", updated!.FullName);
            Assert.False(updated.IsActive);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedPatient = await verificationContext.Patients.SingleAsync();

            Assert.Equal("Ana Sofia", storedPatient.FirstName);
            Assert.False(storedPatient.IsActive);
            Assert.Equal("Mario Lopez", storedPatient.ResponsiblePartyName);
        }

        [Fact]
        public async Task SearchAsync_ReturnsOnlyCurrentTenantMatches()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez", "5551234567");
            await SeedPatientAsync(databaseName, tenantA.Id, "Andres", "Soto", "5555555555");
            await SeedPatientAsync(databaseName, tenantB.Id, "Ana", "Torres", "5550000000");

            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = new PatientQueryService(new EfPatientRepository(context), tenantContext);

            var results = await queryService.SearchAsync("Ana", includeInactive: false, take: 25);

            Assert.Single(results);
            Assert.Equal("Ana Lopez", results[0].FullName);
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

        private static async Task<Patient> SeedPatientAsync(
            string databaseName,
            Guid tenantId,
            string firstName,
            string lastName,
            string phone)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var patient = new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14),
                phone,
                $"{firstName.ToLowerInvariant()}@example.com");

            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            return patient;
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
