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
                    true,
                    "Allergic to latex.",
                    "Maria Lopez",
                    "Mother",
                    "5559990000",
                    PatientSex.Female,
                    "Dentist",
                    PatientMaritalStatus.Single,
                    "Existing patient"));

            Assert.Equal("Ana Lopez", patient.FullName);
            Assert.Equal("Female", patient.Sex);
            Assert.Equal("Dentist", patient.Occupation);
            Assert.Equal("Single", patient.MaritalStatus);
            Assert.Equal("Existing patient", patient.ReferredBy);
            Assert.True(patient.IsActive);
            Assert.True(patient.HasClinicalAlerts);
            Assert.Equal("Allergic to latex.", patient.ClinicalAlertsSummary);

            await using var verificationContext = CreateContext(databaseName, new TenantContext());
            var storedPatient = await verificationContext.Patients.SingleAsync();

            Assert.Equal(tenantA.Id, storedPatient.TenantId);
            Assert.Equal("Ana", storedPatient.FirstName);
            Assert.Equal(PatientSex.Female, storedPatient.Sex);
            Assert.Equal("Dentist", storedPatient.Occupation);
            Assert.Equal(PatientMaritalStatus.Single, storedPatient.MaritalStatus);
            Assert.Equal("Existing patient", storedPatient.ReferredBy);
            Assert.True(storedPatient.HasClinicalAlerts);
            Assert.Equal("Allergic to latex.", storedPatient.ClinicalAlertsSummary);
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
                    false,
                    "Should be cleared",
                    "Mario Lopez",
                    "Father",
                    "5554448888",
                    PatientSex.Female,
                    "Teacher",
                    PatientMaritalStatus.Married,
                    "Community campaign"));

            Assert.NotNull(updated);
            Assert.Equal("Ana Sofia Lopez", updated!.FullName);
            Assert.Equal("Female", updated.Sex);
            Assert.Equal("Teacher", updated.Occupation);
            Assert.Equal("Married", updated.MaritalStatus);
            Assert.Equal("Community campaign", updated.ReferredBy);
            Assert.False(updated.IsActive);
            Assert.False(updated.HasClinicalAlerts);
            Assert.Null(updated.ClinicalAlertsSummary);

            await using var verificationContext = CreateContext(databaseName, tenantContext);
            var storedPatient = await verificationContext.Patients.SingleAsync();

            Assert.Equal("Ana Sofia", storedPatient.FirstName);
            Assert.Equal(PatientSex.Female, storedPatient.Sex);
            Assert.Equal("Teacher", storedPatient.Occupation);
            Assert.Equal(PatientMaritalStatus.Married, storedPatient.MaritalStatus);
            Assert.Equal("Community campaign", storedPatient.ReferredBy);
            Assert.False(storedPatient.IsActive);
            Assert.False(storedPatient.HasClinicalAlerts);
            Assert.Null(storedPatient.ClinicalAlertsSummary);
            Assert.Equal("Mario Lopez", storedPatient.ResponsiblePartyName);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNullForCrossTenantPatientAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez", "5551234567");

            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantB.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = new PatientCommandService(new EfPatientRepository(context), tenantContext);

            var result = await commandService.UpdateAsync(
                patientA.Id,
                new SavePatientCommand(
                    "Ana Sofia",
                    "Lopez",
                    new DateOnly(1991, 2, 14),
                    "5551112222",
                    "ana.sofia@example.com",
                    true,
                    false,
                    null,
                    null,
                    null,
                    null,
                    PatientSex.Female,
                    "Teacher",
                    PatientMaritalStatus.Married,
                    "Community campaign"));

            Assert.Null(result);
        }

        [Fact]
        public async Task SearchAsync_ReturnsOnlyCurrentTenantMatches()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            await SeedPatientAsync(
                databaseName,
                tenantA.Id,
                "Ana",
                "Lopez",
                "5551234567",
                hasClinicalAlerts: true,
                clinicalAlertsSummary: "Requires antibiotic prophylaxis.");
            await SeedPatientAsync(databaseName, tenantA.Id, "Andres", "Soto", "5555555555");
            await SeedPatientAsync(databaseName, tenantB.Id, "Ana", "Torres", "5550000000");

            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = new PatientQueryService(new EfPatientRepository(context), tenantContext);

            var results = await queryService.SearchAsync("Ana", includeInactive: false, take: 25);

            Assert.Single(results);
            Assert.Equal("Ana Lopez", results[0].FullName);
            Assert.True(results[0].HasClinicalAlerts);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsClinicalAlertsSummaryOnlyInDetailView()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(
                databaseName,
                tenantA.Id,
                "Ana",
                "Lopez",
                "5551234567",
                hasClinicalAlerts: true,
                clinicalAlertsSummary: "Requires antibiotic prophylaxis.");

            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = new PatientQueryService(new EfPatientRepository(context), tenantContext);

            var detail = await queryService.GetByIdAsync(patient.Id);
            var results = await queryService.SearchAsync("Ana", includeInactive: false, take: 25);

            Assert.NotNull(detail);
            Assert.True(detail!.HasClinicalAlerts);
            Assert.Equal("Unspecified", detail.Sex);
            Assert.Equal("Unspecified", detail.MaritalStatus);
            Assert.Equal("Requires antibiotic prophylaxis.", detail.ClinicalAlertsSummary);
            Assert.True(results[0].HasClinicalAlerts);
            Assert.Null(typeof(BigSmile.Application.Features.Patients.Dtos.PatientSummaryDto)
                .GetProperty(nameof(BigSmile.Application.Features.Patients.Dtos.PatientDetailDto.ClinicalAlertsSummary)));
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
            string phone,
            bool hasClinicalAlerts = false,
            string? clinicalAlertsSummary = null)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var patient = new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14),
                phone,
                $"{firstName.ToLowerInvariant()}@example.com",
                hasClinicalAlerts: hasClinicalAlerts,
                clinicalAlertsSummary: clinicalAlertsSummary);

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
