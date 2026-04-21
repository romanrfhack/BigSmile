using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;

namespace BigSmile.IntegrationTests.TenantEnforcement
{
    public class AppDbContextTenantEnforcementTests
    {
        [Fact]
        public async Task AuthenticatedTenantScope_ReadsOnlyCurrentTenantRecords()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, await GetSinglePatientIdAsync(databaseName, tenantA.Id));
            await SeedClinicalRecordAsync(databaseName, tenantB.Id, await GetSinglePatientIdAsync(databaseName, tenantB.Id));
            await SeedOdontogramAsync(databaseName, tenantA.Id, await GetSinglePatientIdAsync(databaseName, tenantA.Id));
            await SeedOdontogramAsync(databaseName, tenantB.Id, await GetSinglePatientIdAsync(databaseName, tenantB.Id));
            await SeedAppointmentAsync(databaseName, tenantA.Id, tenantA.Branches.Single().Id, (await GetSinglePatientIdAsync(databaseName, tenantA.Id)), new DateTime(2026, 4, 14, 9, 0, 0), new DateTime(2026, 4, 14, 9, 30, 0));
            await SeedAppointmentAsync(databaseName, tenantB.Id, tenantB.Branches.Single().Id, (await GetSinglePatientIdAsync(databaseName, tenantB.Id)), new DateTime(2026, 4, 14, 10, 0, 0), new DateTime(2026, 4, 14, 10, 30, 0));
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);

            var tenants = await context.Tenants.OrderBy(tenant => tenant.Name).ToListAsync();
            var branches = await context.Branches.OrderBy(branch => branch.Name).ToListAsync();
            var patients = await context.Patients.OrderBy(patient => patient.LastName).ToListAsync();
            var clinicalRecords = await context.ClinicalRecords.OrderBy(clinicalRecord => clinicalRecord.PatientId).ToListAsync();
            var odontograms = await context.Odontograms.OrderBy(odontogram => odontogram.PatientId).ToListAsync();
            var appointments = await context.Appointments.OrderBy(appointment => appointment.StartsAt).ToListAsync();

            Assert.Single(tenants);
            Assert.Single(branches);
            Assert.Single(patients);
            Assert.Single(clinicalRecords);
            Assert.Single(odontograms);
            Assert.Single(appointments);
            Assert.Equal(tenantA.Id, tenants[0].Id);
            Assert.Equal(tenantA.Id, branches[0].TenantId);
            Assert.Equal(tenantA.Id, patients[0].TenantId);
            Assert.Equal(tenantA.Id, clinicalRecords[0].TenantId);
            Assert.Equal(tenantA.Id, odontograms[0].TenantId);
            Assert.Equal(tenantA.Id, appointments[0].TenantId);
            Assert.DoesNotContain(tenants, tenant => tenant.Id == tenantB.Id);
        }

        [Fact]
        public async Task AuthenticatedPlatformScope_WithoutOverride_HidesTenantOwnedData()
        {
            var databaseName = Guid.NewGuid().ToString();
            await SeedTenantsAsync(databaseName);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Platform, isAuthenticated: true);

            await using var context = CreateContext(databaseName, tenantContext);

            Assert.Empty(await context.Tenants.ToListAsync());
            Assert.Empty(await context.Branches.ToListAsync());
            Assert.Empty(await context.ClinicalRecords.ToListAsync());
            Assert.Empty(await context.Odontograms.ToListAsync());
        }

        [Fact]
        public async Task PlatformOverride_ExposesTenantOwnedData()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, await GetSinglePatientIdAsync(databaseName, tenantA.Id));
            await SeedClinicalRecordAsync(databaseName, tenantB.Id, await GetSinglePatientIdAsync(databaseName, tenantB.Id));
            await SeedOdontogramAsync(databaseName, tenantA.Id, await GetSinglePatientIdAsync(databaseName, tenantA.Id));
            await SeedOdontogramAsync(databaseName, tenantB.Id, await GetSinglePatientIdAsync(databaseName, tenantB.Id));
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Platform, isAuthenticated: true);
            tenantContext.EnablePlatformOverride();

            await using var context = CreateContext(databaseName, tenantContext);

            Assert.Equal(2, await context.Tenants.CountAsync());
            Assert.Equal(2, await context.Branches.CountAsync());
            Assert.Equal(2, await context.Patients.CountAsync());
            Assert.Equal(2, await context.ClinicalRecords.CountAsync());
            Assert.Equal(2, await context.Odontograms.CountAsync());
        }

        [Fact]
        public async Task TenantScopedWrite_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var foreignTenant = new Tenant("Foreign Tenant", "foreign");

            context.Tenants.Add(foreignTenant);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("does not match the current tenant context", exception.Message);
        }

        [Fact]
        public async Task TenantScopedWrite_AllowsCurrentTenantMutation()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            var branch = await context.Branches.SingleAsync();
            branch.UpdateName("Updated Branch");

            await context.SaveChangesAsync();

            Assert.Equal("Updated Branch", (await context.Branches.SingleAsync()).Name);
        }

        [Fact]
        public async Task TenantScopedWrite_BlocksCrossTenantPatientWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            context.Patients.Add(new Patient(
                tenantB.Id,
                "Foreign",
                "Patient",
                new DateOnly(1991, 2, 14),
                "5551234567",
                "foreign@example.com"));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("does not match the current tenant context", exception.Message);
        }

        [Fact]
        public async Task TenantScopedWrite_BlocksCrossTenantAppointmentWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            context.Appointments.Add(new Appointment(
                tenantB.Id,
                tenantB.Branches.Single().Id,
                patientA.Id,
                new DateTime(2026, 4, 14, 9, 0, 0),
                new DateTime(2026, 4, 14, 9, 30, 0),
                null));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("does not match the current tenant context", exception.Message);
        }

        [Fact]
        public async Task TenantScopedWrite_BlocksCrossTenantClinicalRecordWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            context.ClinicalRecords.Add(new ClinicalRecord(
                tenantB.Id,
                patientA.Id,
                Guid.NewGuid(),
                "Foreign clinical background.",
                null));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("does not match the current tenant context", exception.Message);
        }

        [Fact]
        public async Task TenantScopedWrite_BlocksCrossTenantOdontogramWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(Guid.NewGuid().ToString(), AccessScope.Tenant, isAuthenticated: true, tenantA.Id.ToString());

            await using var context = CreateContext(databaseName, tenantContext);
            context.Odontograms.Add(new Domain.Entities.Odontogram(
                tenantB.Id,
                patientA.Id,
                Guid.NewGuid()));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
            Assert.Contains("does not match the current tenant context", exception.Message);
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

        private static async Task SeedAppointmentAsync(
            string databaseName,
            Guid tenantId,
            Guid branchId,
            Guid patientId,
            DateTime startsAt,
            DateTime endsAt)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            context.Appointments.Add(new Appointment(tenantId, branchId, patientId, startsAt, endsAt, null));
            await context.SaveChangesAsync();
        }

        private static async Task SeedClinicalRecordAsync(string databaseName, Guid tenantId, Guid patientId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            context.ClinicalRecords.Add(new ClinicalRecord(
                tenantId,
                patientId,
                Guid.NewGuid(),
                "Seeded background.",
                "Seeded medications."));
            await context.SaveChangesAsync();
        }

        private static async Task SeedOdontogramAsync(string databaseName, Guid tenantId, Guid patientId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            context.Odontograms.Add(new Domain.Entities.Odontogram(
                tenantId,
                patientId,
                Guid.NewGuid()));
            await context.SaveChangesAsync();
        }

        private static async Task<Guid> GetSinglePatientIdAsync(string databaseName, Guid tenantId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            return await context.Patients
                .Where(patient => patient.TenantId == tenantId)
                .Select(patient => patient.Id)
                .SingleAsync();
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
