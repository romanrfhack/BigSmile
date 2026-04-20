using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.Clinical
{
    public class ClinicalRecordServicesTests
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

            var created = await commandService.CreateAsync(
                patient.Id,
                new SaveClinicalRecordSnapshotCommand(
                    "History of bruxism.",
                    "Ibuprofen as needed.",
                    Array.Empty<ClinicalAllergyInput>()));

            Assert.Equal(patient.Id, created.PatientId);
            Assert.Empty(created.Allergies);
            Assert.Empty(created.Notes);
            Assert.Empty(created.Timeline);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal("History of bruxism.", loaded!.MedicalBackgroundSummary);
            Assert.Empty(loaded.Allergies);
            Assert.Empty(loaded.Timeline);
        }

        [Fact]
        public async Task CreateAsync_PersistsInitialAllergySnapshot()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var createContext = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(createContext, tenantContext);

            await commandService.CreateAsync(
                patient.Id,
                new SaveClinicalRecordSnapshotCommand(
                    "Background summary.",
                    "Current medications summary.",
                    new[]
                    {
                        new ClinicalAllergyInput("Latex", "Rash", "Use nitrile gloves.")
                    }));

            await using var queryContext = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(queryContext, tenantContext);
            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Single(loaded!.Allergies);
            Assert.Equal("Latex", loaded.Allergies[0].Substance);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_WhenClinicalRecordDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var clinicalRecord = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.Null(clinicalRecord);
        }

        [Fact]
        public async Task CreateAsync_Fails_WhenClinicalRecordAlreadyExistsForPatient()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var actorUserId = Guid.NewGuid();
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(
                patient.Id,
                new SaveClinicalRecordSnapshotCommand(null, null, Array.Empty<ClinicalAllergyInput>())));

            Assert.Contains("already exists", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateAsync_BlocksPatientFromAnotherTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.CreateAsync(
                foreignPatient.Id,
                new SaveClinicalRecordSnapshotCommand(null, null, Array.Empty<ClinicalAllergyInput>())));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetByPatientIdAsync_ReturnsNull_ForCrossTenantAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, patientA.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var clinicalRecord = await queryService.GetByPatientIdAsync(patientA.Id);

            Assert.Null(clinicalRecord);
        }

        [Fact]
        public async Task AddDiagnosisAsync_SucceedsWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var updated = await commandService.AddDiagnosisAsync(
                patient.Id,
                new AddClinicalDiagnosisCommand("Occlusal caries", "Upper molar."));

            Assert.Single(updated.Diagnoses);
            Assert.Equal("Occlusal caries", updated.Diagnoses[0].DiagnosisText);
            Assert.Equal("Active", updated.Diagnoses[0].Status);
        }

        [Fact]
        public async Task ResolveDiagnosisAsync_SucceedsWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var writeContext = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(writeContext, tenantContext);
            var withDiagnosis = await commandService.AddDiagnosisAsync(
                patient.Id,
                new AddClinicalDiagnosisCommand("Occlusal caries", null));

            var diagnosisId = withDiagnosis.Diagnoses.Single().DiagnosisId;

            await using var resolveContext = CreateContext(databaseName, tenantContext);
            var resolveService = CreateCommandService(resolveContext, tenantContext);
            var resolved = await resolveService.ResolveDiagnosisAsync(patient.Id, diagnosisId);

            var diagnosis = Assert.Single(resolved.Diagnoses);
            Assert.Equal("Resolved", diagnosis.Status);
            Assert.NotNull(diagnosis.ResolvedAtUtc);
            Assert.NotNull(diagnosis.ResolvedByUserId);
        }

        [Fact]
        public async Task GetByPatientIdAsync_IncludesDiagnosesOrderedActiveFirstAndNewestFirst()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var writeContext = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(writeContext, tenantContext);
            await commandService.AddDiagnosisAsync(patient.Id, new AddClinicalDiagnosisCommand("Older active", null));
            var middleDiagnosis = await commandService.AddDiagnosisAsync(patient.Id, new AddClinicalDiagnosisCommand("Resolved middle", null));
            var middleDiagnosisId = middleDiagnosis.Diagnoses
                .Single(diagnosis => diagnosis.DiagnosisText == "Resolved middle")
                .DiagnosisId;
            await commandService.ResolveDiagnosisAsync(patient.Id, middleDiagnosisId);
            await commandService.AddDiagnosisAsync(patient.Id, new AddClinicalDiagnosisCommand("Newest active", null));

            await using var queryContext = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(queryContext, tenantContext);
            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal(
                new[] { "Newest active", "Older active", "Resolved middle" },
                loaded!.Diagnoses.Select(diagnosis => diagnosis.DiagnosisText).ToArray());
            Assert.Equal(new[] { "Active", "Active", "Resolved" }, loaded.Diagnoses.Select(diagnosis => diagnosis.Status).ToArray());
        }

        [Fact]
        public async Task GetByPatientIdAsync_IncludesClinicalTimelineNewestFirst()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordWithTimelineAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var loaded = await queryService.GetByPatientIdAsync(patient.Id);

            Assert.NotNull(loaded);
            Assert.Equal(
                new[]
                {
                    "ClinicalDiagnosisResolved",
                    "ClinicalDiagnosisCreated",
                    "ClinicalDiagnosisCreated",
                    "ClinicalNoteCreated"
                },
                loaded!.Timeline.Select(entry => entry.EventType).ToArray());
            Assert.Equal(
                new[]
                {
                    "Diagnosis resolved",
                    "Diagnosis added",
                    "Diagnosis added",
                    "Clinical note added"
                },
                loaded.Timeline.Select(entry => entry.Title).ToArray());
        }

        [Fact]
        public async Task GetByPatientIdAsync_TimelineRemainsBlockedForCrossTenantAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordWithTimelineAsync(databaseName, tenantA.Id, patientA.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var clinicalRecord = await queryService.GetByPatientIdAsync(patientA.Id);

            Assert.Null(clinicalRecord);
        }

        [Fact]
        public async Task AddNoteAsync_Fails_WhenClinicalRecordDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddNoteAsync(
                patient.Id,
                new AddClinicalNoteCommand("Attempting to add note without a record.")));

            Assert.Contains("must be created explicitly", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddDiagnosisAsync_Fails_WhenClinicalRecordDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddDiagnosisAsync(
                patient.Id,
                new AddClinicalDiagnosisCommand("Occlusal caries", null)));

            Assert.Contains("created explicitly", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task AddDiagnosisAsync_BlocksCrossTenantWrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var foreignPatient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            await SeedClinicalRecordAsync(databaseName, tenantB.Id, foreignPatient.Id, actorUserId);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.AddDiagnosisAsync(
                foreignPatient.Id,
                new AddClinicalDiagnosisCommand("Occlusal caries", null)));

            Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ResolveDiagnosisAsync_Fails_WhenDiagnosisDoesNotExistInClinicalRecord()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            await SeedClinicalRecordAsync(databaseName, tenantA.Id, patient.Id, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var commandService = CreateCommandService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.ResolveDiagnosisAsync(
                patient.Id,
                Guid.NewGuid()));

            Assert.Contains("does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task PlatformScopedClinicalAccessWithoutTenantContext_IsBlocked()
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

        private static ClinicalRecordCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext)
        {
            return new ClinicalRecordCommandService(
                new EfClinicalRecordRepository(context),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static ClinicalRecordQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new ClinicalRecordQueryService(
                new EfClinicalRecordRepository(context),
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

        private static async Task<ClinicalRecord> SeedClinicalRecordAsync(string databaseName, Guid tenantId, Guid patientId, Guid actorUserId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var clinicalRecord = new ClinicalRecord(
                tenantId,
                patientId,
                actorUserId,
                "Seeded background.",
                "Seeded medications.");

            context.ClinicalRecords.Add(clinicalRecord);
            await context.SaveChangesAsync();

            return clinicalRecord;
        }

        private static async Task SeedClinicalRecordWithTimelineAsync(string databaseName, Guid tenantId, Guid patientId, Guid actorUserId)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var clinicalRecord = new ClinicalRecord(
                tenantId,
                patientId,
                actorUserId,
                "Seeded background.",
                "Seeded medications.");

            var note = clinicalRecord.AddClinicalNote("Clinical note summary.", actorUserId);
            SetCreatedAt(note, new DateTime(2026, 4, 20, 9, 0, 0, DateTimeKind.Utc));

            var createdDiagnosis = clinicalRecord.AddDiagnosis("Occlusal caries", "Upper molar.", actorUserId);
            SetCreatedAt(createdDiagnosis, new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc));

            var resolvedDiagnosis = clinicalRecord.AddDiagnosis("Gingivitis", null, actorUserId);
            SetCreatedAt(resolvedDiagnosis, new DateTime(2026, 4, 20, 11, 0, 0, DateTimeKind.Utc));
            clinicalRecord.ResolveDiagnosis(resolvedDiagnosis.Id, actorUserId);
            SetResolvedAt(resolvedDiagnosis, new DateTime(2026, 4, 20, 12, 0, 0, DateTimeKind.Utc), actorUserId);

            context.ClinicalRecords.Add(clinicalRecord);
            await context.SaveChangesAsync();
        }

        private static void SetCreatedAt(ClinicalNote note, DateTime value)
        {
            var field = typeof(ClinicalNote)
                .GetField($"<{nameof(ClinicalNote.CreatedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(note, value);
        }

        private static void SetCreatedAt(ClinicalDiagnosis diagnosis, DateTime value)
        {
            var field = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.CreatedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            field.SetValue(diagnosis, value);
        }

        private static void SetResolvedAt(ClinicalDiagnosis diagnosis, DateTime value, Guid resolvedByUserId)
        {
            var resolvedAtField = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.ResolvedAtUtc)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
            var resolvedByField = typeof(ClinicalDiagnosis)
                .GetField($"<{nameof(ClinicalDiagnosis.ResolvedByUserId)}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            resolvedAtField.SetValue(diagnosis, value);
            resolvedByField.SetValue(diagnosis, resolvedByUserId);
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
