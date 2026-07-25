using BigSmile.Application.Features.PatientIntakes.Dtos;
using BigSmile.Application.Features.PatientIntakes.Services;
using BigSmile.Application.Interfaces.PatientIntakes;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.PatientPortal
{
    public sealed class PatientIntakeSelfServiceTests
    {
        private static readonly DateTime InitialUtc =
            new(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task GetCurrentAsync_WhenMissing_HasNoWriteSideEffects()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identity = BuildIdentity(seed);

            await using (var context = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var service = CreateService(context, timeProvider);

                var result = await service.GetCurrentAsync(identity);

                Assert.False(result.Succeeded);
                Assert.Equal(PatientIntakeReadFailure.Missing, result.Failure);
            }

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            Assert.Empty(await verificationContext.PatientIntakes.ToListAsync());
            Assert.Empty(await verificationContext.PatientIntakeMedicalAnswers.ToListAsync());
            Assert.Empty(await verificationContext.PatientIntakeRevisions.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_PrefillsApprovedPatientFieldsAndRejectsSecondActiveDraft()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identity = BuildIdentity(seed);

            PatientIntakeDto created;
            await using (var context = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var service = CreateService(context, timeProvider);
                var result = await service.CreateAsync(identity);

                Assert.True(result.Succeeded);
                created = Assert.IsType<PatientIntakeDto>(result.Intake);
                Assert.Equal(PatientIntakeOrigin.ExistingPatientPortal.ToString(), created.Origin);
                Assert.Equal(PatientIntakeStatus.Draft.ToString(), created.Status);
                Assert.Equal(seed.Patient.FirstName, created.FirstName);
                Assert.Equal(seed.Patient.LastName, created.LastName);
                Assert.Equal(seed.Patient.DateOfBirth, created.DateOfBirth);
                Assert.Equal(seed.Patient.PrimaryPhone, created.PreferredPhone);
                Assert.Equal(seed.Patient.Email, created.Email);
                Assert.Equal(39, created.MedicalAnswers.Count);
                Assert.All(created.MedicalAnswers, answer =>
                    Assert.Equal(ClinicalMedicalAnswerValue.Unknown.ToString(), answer.Answer));
                Assert.Equal(0, created.CurrentRevisionNumber);
                Assert.NotEmpty(created.ConcurrencyToken);
                Assert.Equal(InitialUtc.AddDays(30), created.ExpiresAtUtc);
            }

            await using (var duplicateContext = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var duplicateService = CreateService(duplicateContext, timeProvider);
                var duplicate = await duplicateService.CreateAsync(identity);

                Assert.False(duplicate.Succeeded);
                Assert.Equal(
                    PatientIntakeCreateFailure.ActiveDraftExists,
                    duplicate.Failure);
            }

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            var intake = Assert.Single(await verificationContext.PatientIntakes.ToListAsync());
            Assert.Equal(seed.Account.Id, intake.PatientPortalAccountId);
            Assert.Equal(seed.Patient.Id, intake.PatientId);
            Assert.Equal(39, await verificationContext.PatientIntakeMedicalAnswers.CountAsync());
            Assert.Empty(await verificationContext.PatientIntakeRevisions.ToListAsync());

            var patient = await verificationContext.Patients.SingleAsync();
            Assert.Equal("Ana", patient.FirstName);
            Assert.Equal("555-0100", patient.PrimaryPhone);
            Assert.Equal("ana@example.com", patient.Email);
        }

        [Fact]
        public async Task SaveAsync_IdenticalNormalizedSnapshotIsNoOp()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identity = BuildIdentity(seed);
            var created = await CreateDraftAsync(databaseName, identity, timeProvider);
            var expectedExpiry = created.ExpiresAtUtc;

            timeProvider.Advance(TimeSpan.FromHours(4));
            PatientIntakeSaveResult result;
            await using (var context = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var service = CreateService(context, timeProvider);
                result = await service.SaveAsync(
                    identity,
                    BuildSaveCommand(created),
                    "no-op-save");
            }

            Assert.True(result.Succeeded);
            Assert.False(result.Changed);
            Assert.Equal(0, result.Intake!.CurrentRevisionNumber);
            Assert.Equal(expectedExpiry, result.Intake.ExpiresAtUtc);
            Assert.Equal(created.ConcurrencyToken, result.Intake.ConcurrencyToken);

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            var intake = await verificationContext.PatientIntakes.SingleAsync();
            Assert.Null(intake.LastEffectiveSavedAtUtc);
            Assert.Equal(expectedExpiry, intake.ExpiresAtUtc);
            Assert.Empty(await verificationContext.PatientIntakeRevisions.ToListAsync());
        }

        [Fact]
        public async Task SaveAsync_EffectiveChangeCreatesOneRevisionAndLeavesCanonicalDataUntouched()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identity = BuildIdentity(seed);
            var created = await CreateDraftAsync(databaseName, identity, timeProvider);
            timeProvider.Advance(TimeSpan.FromDays(1));

            var changedAnswers = created.MedicalAnswers
                .Select(answer => answer.QuestionKey == "diabetes"
                    ? new SavePatientIntakeMedicalAnswerCommand(
                        answer.QuestionKey,
                        ClinicalMedicalAnswerValue.Yes,
                        "Controlled with diet")
                    : new SavePatientIntakeMedicalAnswerCommand(
                        answer.QuestionKey,
                        Enum.Parse<ClinicalMedicalAnswerValue>(answer.Answer),
                        answer.Details))
                .ToArray();
            var command = BuildSaveCommand(created) with
            {
                MobilePhone = "555-0200",
                ReasonForVisit = "Pain while chewing",
                MedicalAnswers = changedAnswers
            };

            PatientIntakeSaveResult result;
            await using (var context = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var service = CreateService(context, timeProvider);
                result = await service.SaveAsync(
                    identity,
                    command,
                    "effective-save-1");
            }

            Assert.True(result.Succeeded);
            Assert.True(result.Changed);
            Assert.Equal(1, result.Intake!.CurrentRevisionNumber);
            Assert.Equal("555-0200", result.Intake.MobilePhone);
            Assert.Equal("Pain while chewing", result.Intake.ReasonForVisit);
            Assert.Equal(InitialUtc.AddDays(31), result.Intake.ExpiresAtUtc);
            Assert.NotEqual(created.ConcurrencyToken, result.Intake.ConcurrencyToken);
            Assert.Contains(
                result.Intake.MedicalAnswers,
                answer => answer.QuestionKey == "diabetes" &&
                          answer.Answer == ClinicalMedicalAnswerValue.Yes.ToString() &&
                          answer.Details == "Controlled with diet");

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            var revision = Assert.Single(await verificationContext.PatientIntakeRevisions.ToListAsync());
            Assert.Equal(1, revision.RevisionNumber);
            Assert.Equal(seed.Account.Id, revision.ActorPatientPortalAccountId);
            Assert.Equal("effective-save-1", revision.CorrelationId);
            Assert.Contains("mobilePhone", revision.ChangedFieldsJson, StringComparison.Ordinal);
            Assert.Contains("reasonForVisit", revision.ChangedFieldsJson, StringComparison.Ordinal);
            Assert.Contains("medicalAnswers.diabetes", revision.ChangedFieldsJson, StringComparison.Ordinal);

            var canonicalPatient = await verificationContext.Patients.SingleAsync();
            Assert.Equal("Ana", canonicalPatient.FirstName);
            Assert.Equal("555-0100", canonicalPatient.PrimaryPhone);
            Assert.Equal("ana@example.com", canonicalPatient.Email);
            Assert.Empty(await verificationContext.ClinicalMedicalAnswers.ToListAsync());
            Assert.Empty(await verificationContext.ClinicalRecords.ToListAsync());
        }

        [Fact]
        public async Task SaveAsync_RejectsStaleConcurrencyTokenWithoutOverwrite()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identity = BuildIdentity(seed);
            var created = await CreateDraftAsync(databaseName, identity, timeProvider);

            PatientIntakeSaveResult firstSave;
            await using (var firstContext = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                firstSave = await CreateService(firstContext, timeProvider).SaveAsync(
                    identity,
                    BuildSaveCommand(created) with { ReasonForVisit = "First value" },
                    "first-save");
            }
            Assert.True(firstSave.Succeeded);

            await using (var staleContext = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var stale = await CreateService(staleContext, timeProvider).SaveAsync(
                    identity,
                    BuildSaveCommand(created) with { ReasonForVisit = "Stale value" },
                    "stale-save");

                Assert.False(stale.Succeeded);
                Assert.Equal(
                    PatientIntakeSaveFailure.ConcurrentConflict,
                    stale.Failure);
            }

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            var intake = await verificationContext.PatientIntakes
                .Include(item => item.MedicalAnswers)
                .SingleAsync();
            Assert.Equal("First value", intake.ReasonForVisit);
            Assert.Equal(1, intake.CurrentRevisionNumber);
            Assert.Single(await verificationContext.PatientIntakeRevisions.ToListAsync());
        }

        [Fact]
        public async Task ExpiredDraft_IsSoftExpiredAndExplicitCreateStartsReplacement()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identity = BuildIdentity(seed);
            var settings = new FixedDraftSettings(TimeSpan.FromMinutes(30));
            var created = await CreateDraftAsync(
                databaseName,
                identity,
                timeProvider,
                settings);
            timeProvider.Advance(TimeSpan.FromMinutes(31));

            await using (var getContext = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var missing = await CreateService(getContext, timeProvider, settings)
                    .GetCurrentAsync(identity);
                Assert.Equal(PatientIntakeReadFailure.Missing, missing.Failure);
            }

            await using (var createContext = CreateContext(
                             databaseName,
                             CreatePatientContext(identity)))
            {
                var replacement = await CreateService(createContext, timeProvider, settings)
                    .CreateAsync(identity);
                Assert.True(replacement.Succeeded);
                Assert.NotEqual(created.CreatedAtUtc, replacement.Intake!.CreatedAtUtc);
                Assert.Equal(timeProvider.GetUtcNow().UtcDateTime, replacement.Intake.CreatedAtUtc);
            }

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            var intakes = await verificationContext.PatientIntakes
                .OrderBy(intake => intake.CreatedAtUtc)
                .ToListAsync();
            Assert.Equal(2, intakes.Count);
            Assert.Equal(PatientIntakeStatus.Expired, intakes[0].Status);
            Assert.Equal(PatientIntakeStatus.Draft, intakes[1].Status);
            Assert.Equal(seed.Account.Id, intakes[0].PatientPortalAccountId);
            Assert.Equal(seed.Account.Id, intakes[1].PatientPortalAccountId);
        }

        [Fact]
        public async Task SessionIdentityMismatchCannotReadCreateOrSaveAnotherAccountDraft()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedLinkedAccountAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedLinkedAccountAsync(databaseName, "Tenant B", "tenant-b");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var identityA = BuildIdentity(tenantA);
            var createdA = await CreateDraftAsync(databaseName, identityA, timeProvider);
            var forgedIdentity = new PatientPortalSessionIdentity(
                tenantA.Account.Id,
                tenantB.Tenant.Id,
                tenantA.Patient.Id,
                tenantA.Account.SessionVersion);

            await using var context = CreateContext(
                databaseName,
                CreatePatientContext(forgedIdentity));
            var service = CreateService(context, timeProvider);

            Assert.Equal(
                PatientIntakeReadFailure.SessionInvalid,
                (await service.GetCurrentAsync(forgedIdentity)).Failure);
            Assert.Equal(
                PatientIntakeCreateFailure.SessionInvalid,
                (await service.CreateAsync(forgedIdentity)).Failure);
            Assert.Equal(
                PatientIntakeSaveFailure.SessionInvalid,
                (await service.SaveAsync(
                    forgedIdentity,
                    BuildSaveCommand(createdA),
                    "forged-save")).Failure);
        }

        private static async Task<PatientIntakeDto> CreateDraftAsync(
            string databaseName,
            PatientPortalSessionIdentity identity,
            MutableTimeProvider timeProvider,
            IPatientIntakeDraftSettings? settings = null)
        {
            await using var context = CreateContext(
                databaseName,
                CreatePatientContext(identity));
            var result = await CreateService(context, timeProvider, settings)
                .CreateAsync(identity);
            Assert.True(result.Succeeded);
            return result.Intake!;
        }

        private static PatientIntakeSelfService CreateService(
            AppDbContext context,
            TimeProvider timeProvider,
            IPatientIntakeDraftSettings? settings = null)
        {
            return new PatientIntakeSelfService(
                new EfPatientPortalAuthenticationRepository(context),
                new EfPatientIntakeRepository(context),
                settings ?? new FixedDraftSettings(TimeSpan.FromDays(30)),
                timeProvider);
        }

        private static SavePatientIntakeDraftCommand BuildSaveCommand(PatientIntakeDto intake)
        {
            return new SavePatientIntakeDraftCommand(
                intake.FirstName,
                intake.LastName,
                intake.DateOfBirth,
                Enum.Parse<PatientSex>(intake.Sex),
                intake.Occupation,
                Enum.Parse<PatientMaritalStatus>(intake.MaritalStatus),
                intake.ReferredBy,
                intake.PreferredPhone,
                intake.MobilePhone,
                intake.HomePhone,
                intake.WorkPhone,
                intake.Email,
                intake.ResponsiblePartyName,
                intake.ResponsiblePartyRelationship,
                intake.ResponsiblePartyPhone,
                intake.ReasonForVisit,
                intake.MedicalAnswers
                    .Select(answer => new SavePatientIntakeMedicalAnswerCommand(
                        answer.QuestionKey,
                        Enum.Parse<ClinicalMedicalAnswerValue>(answer.Answer),
                        answer.Details))
                    .ToArray(),
                intake.ConcurrencyToken);
        }

        private static PatientPortalSessionIdentity BuildIdentity(SeedData seed)
        {
            return new PatientPortalSessionIdentity(
                seed.Account.Id,
                seed.Tenant.Id,
                seed.Patient.Id,
                seed.Account.SessionVersion);
        }

        private static TenantContext CreatePatientContext(
            PatientPortalSessionIdentity identity)
        {
            var context = new TenantContext();
            context.SetRequestContext(
                identity.AccountId.ToString(),
                AccessScope.Patient,
                isAuthenticated: true,
                identity.TenantId.ToString());
            return context;
        }

        private static async Task<SeedData> SeedLinkedAccountAsync(
            string databaseName,
            string tenantName,
            string subdomain)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenant = new Tenant(tenantName, subdomain);
            var patient = new Patient(
                tenant.Id,
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14),
                primaryPhone: "555-0100",
                email: "ana@example.com",
                responsiblePartyName: "Laura Lopez",
                responsiblePartyRelationship: "Mother",
                responsiblePartyPhone: "555-0101",
                sex: PatientSex.Female,
                occupation: "Designer",
                maritalStatus: PatientMaritalStatus.Single,
                referredBy: "Friend");
            var account = PatientPortalAccount.CreateForExistingPatient(
                patient,
                $"{subdomain}.patient",
                "versioned-password-hash");

            context.Tenants.Add(tenant);
            context.Patients.Add(patient);
            context.PatientPortalAccounts.Add(account);
            await context.SaveChangesAsync();

            return new SeedData(tenant, patient, account);
        }

        private static AppDbContext CreateContext(
            string databaseName,
            TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var configuration = new ConfigurationBuilder().Build();
            return new AppDbContext(options, configuration, tenantContext);
        }

        private sealed record SeedData(
            Tenant Tenant,
            Patient Patient,
            PatientPortalAccount Account);

        private sealed class FixedDraftSettings : IPatientIntakeDraftSettings
        {
            public FixedDraftSettings(TimeSpan draftLifetime)
            {
                DraftLifetime = draftLifetime;
            }

            public TimeSpan DraftLifetime { get; }
        }

        private sealed class MutableTimeProvider : TimeProvider
        {
            private DateTimeOffset _utcNow;

            public MutableTimeProvider(DateTime initialUtc)
            {
                _utcNow = new DateTimeOffset(initialUtc);
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;

            public void Advance(TimeSpan duration)
            {
                _utcNow = _utcNow.Add(duration);
            }
        }
    }
}
