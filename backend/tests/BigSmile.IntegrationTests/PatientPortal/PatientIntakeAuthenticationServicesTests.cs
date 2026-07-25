using BigSmile.Application.Features.PatientIntakeAuthentication.Commands;
using BigSmile.Application.Features.PatientIntakeAuthentication.Dtos;
using BigSmile.Application.Features.PatientIntakes.Dtos;
using BigSmile.Application.Features.PatientIntakes.Services;
using BigSmile.Application.Interfaces.PatientIntakes;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.PatientPortal
{
    public sealed class PatientIntakeAuthenticationServicesTests
    {
        private static readonly DateTime InitialUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);
        private static readonly FixedAuthenticationSettings AuthenticationSettings = new();
        private static readonly FixedJwtSettings JwtSettings = new();
        private static readonly FixedDraftSettings DraftSettings = new();

        [Fact]
        public async Task ActivateAsync_AtomicallyCreatesUnlinkedAccountDraftAndRejectsReplay()
        {
            var databaseName = Guid.NewGuid().ToString();
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();
            var seed = await SeedWaitingRoomLinkAsync(
                databaseName,
                tokenService,
                expiresAtUtc: InitialUtc.AddMinutes(30));

            PatientIntakeAuthenticationResponseDto authentication;
            await using (var activationContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var service = CreatePublicService(
                    activationContext,
                    timeProvider,
                    tokenService);
                var result = await service.ActivateAsync(
                    new ActivatePatientIntakeAccountCommand(
                        seed.RawToken,
                        "new.patient",
                        "A sufficiently long waiting-room password."),
                    "intake-activate-1");

                Assert.True(result.Succeeded);
                authentication = Assert.IsType<PatientIntakeAuthenticationResponseDto>(
                    result.Authentication);
                Assert.Equal("tenant-a", authentication.Current.TenantSubdomain);
            }

            await using (var verificationContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var account = await verificationContext.PatientPortalAccounts.SingleAsync();
                var intake = await verificationContext.PatientIntakes
                    .Include(item => item.MedicalAnswers)
                    .SingleAsync();
                var link = await verificationContext.PatientIntakeAccessLinks.SingleAsync();
                var linkAudits = await verificationContext.PatientIntakeAccessLinkAuditEntries
                    .OrderBy(entry => entry.OccurredAtUtc)
                    .ToListAsync();
                var authAudits = await verificationContext.PatientIntakeAuthenticationAuditEntries
                    .OrderBy(entry => entry.Action)
                    .ToListAsync();

                Assert.Null(account.PatientId);
                Assert.True(account.IsActive);
                Assert.NotEqual(
                    "A sufficiently long waiting-room password.",
                    account.PasswordHash);
                Assert.Equal(account.Id, intake.PatientPortalAccountId);
                Assert.Null(intake.PatientId);
                Assert.Equal(PatientIntakeOrigin.NewPatientWaitingRoom, intake.Origin);
                Assert.Equal(seed.Branch.Id, intake.BranchId);
                Assert.Equal(39, intake.MedicalAnswers.Count);
                Assert.All(
                    intake.MedicalAnswers,
                    answer => Assert.Equal(
                        ClinicalMedicalAnswerValue.Unknown,
                        answer.Answer));
                Assert.Equal(account.Id, link.ConsumedByPatientPortalAccountId);
                Assert.Equal(InitialUtc, link.ConsumedAtUtc);
                Assert.Contains(
                    linkAudits,
                    entry => entry.Action == PatientIntakeAccessLinkAuditAction.Consumed);
                Assert.Contains(
                    authAudits,
                    entry => entry.Action == PatientIntakeAuthenticationAuditAction.AccountActivated);
                Assert.Contains(
                    authAudits,
                    entry => entry.Action == PatientIntakeAuthenticationAuditAction.LinkConsumed);
                Assert.Empty(await verificationContext.Patients.ToListAsync());
                Assert.Empty(await verificationContext.ClinicalRecords.ToListAsync());
                Assert.Empty(await verificationContext.ClinicalMedicalAnswers.ToListAsync());
            }

            await using (var replayContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var replay = await CreatePublicService(
                        replayContext,
                        timeProvider,
                        tokenService)
                    .ActivateAsync(
                        new ActivatePatientIntakeAccountCommand(
                            seed.RawToken,
                            "other.patient",
                            "Another sufficiently long password value."),
                        "intake-replay");

                Assert.False(replay.Succeeded);
                Assert.Equal(
                    PatientIntakeActivationFailure.InvalidActivation,
                    replay.Failure);
            }

            await using (var countContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                Assert.Single(await countContext.PatientPortalAccounts.ToListAsync());
                Assert.Single(await countContext.PatientIntakes.ToListAsync());
            }
        }

        [Fact]
        public async Task ActivateAsync_RejectsUnknownExpiredAndRevokedCredentialsGenerically()
        {
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();

            var unknownDatabase = Guid.NewGuid().ToString();
            await using (var unknownContext = CreateContext(
                             unknownDatabase,
                             new TenantContext()))
            {
                var result = await CreatePublicService(
                        unknownContext,
                        timeProvider,
                        tokenService)
                    .ActivateAsync(
                        new ActivatePatientIntakeAccountCommand(
                            "unknown-waiting-room-token",
                            "unknown.patient",
                            "A sufficiently long password value."),
                        "unknown-token");
                Assert.Equal(
                    PatientIntakeActivationFailure.InvalidActivation,
                    result.Failure);
            }

            var expiredDatabase = Guid.NewGuid().ToString();
            var expired = await SeedWaitingRoomLinkAsync(
                expiredDatabase,
                tokenService,
                InitialUtc.AddMinutes(1));
            timeProvider.Advance(TimeSpan.FromMinutes(2));
            await using (var expiredContext = CreateContext(
                             expiredDatabase,
                             new TenantContext()))
            {
                var result = await CreatePublicService(
                        expiredContext,
                        timeProvider,
                        tokenService)
                    .ActivateAsync(
                        new ActivatePatientIntakeAccountCommand(
                            expired.RawToken,
                            "expired.patient",
                            "A sufficiently long password value."),
                        "expired-token");
                Assert.Equal(
                    PatientIntakeActivationFailure.InvalidActivation,
                    result.Failure);
            }

            timeProvider.Set(InitialUtc);
            var revokedDatabase = Guid.NewGuid().ToString();
            var revoked = await SeedWaitingRoomLinkAsync(
                revokedDatabase,
                tokenService,
                InitialUtc.AddMinutes(30),
                revokeAtUtc: InitialUtc.AddMinutes(1));
            timeProvider.Advance(TimeSpan.FromMinutes(2));
            await using (var revokedContext = CreateContext(
                             revokedDatabase,
                             new TenantContext()))
            {
                var result = await CreatePublicService(
                        revokedContext,
                        timeProvider,
                        tokenService)
                    .ActivateAsync(
                        new ActivatePatientIntakeAccountCommand(
                            revoked.RawToken,
                            "revoked.patient",
                            "A sufficiently long password value."),
                        "revoked-token");
                Assert.Equal(
                    PatientIntakeActivationFailure.InvalidActivation,
                    result.Failure);
            }
        }

        [Fact]
        public async Task LoginSessionLogoutAndSelfSave_AreBoundToExactIntake()
        {
            var databaseName = Guid.NewGuid().ToString();
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();
            var seed = await SeedWaitingRoomLinkAsync(
                databaseName,
                tokenService,
                InitialUtc.AddMinutes(30));
            var authentication = await ActivateAsync(
                databaseName,
                seed.RawToken,
                timeProvider,
                tokenService,
                "resume.patient");
            var identity = new PatientIntakeSessionIdentity(
                authentication.Current.AccountId,
                seed.Tenant.Id,
                authentication.Current.IntakeId,
                authentication.Current.SessionVersion);

            await using (var validationContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var validator = new PatientIntakeSessionValidator(
                    new EfPatientIntakeAuthenticationRepository(validationContext),
                    timeProvider);
                Assert.True(await validator.ValidateAsync(identity));
                Assert.False(await validator.ValidateAsync(identity with
                {
                    TenantId = Guid.NewGuid()
                }));
                Assert.False(await validator.ValidateAsync(identity with
                {
                    IntakeId = Guid.NewGuid()
                }));
            }

            await using (var loginContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var login = await CreatePublicService(
                        loginContext,
                        timeProvider,
                        tokenService)
                    .LoginAsync(
                        new LoginPatientIntakeAccountCommand(
                            "tenant-a",
                            "resume.patient",
                            "A sufficiently long waiting-room password."),
                        "intake-login");
                Assert.NotNull(login);
                Assert.Equal(identity.IntakeId, login!.Current.IntakeId);
            }

            var tenantContext = CreateIntakeContext(identity);
            await using (var selfContext = CreateContext(
                             databaseName,
                             tenantContext))
            {
                var selfService = CreateSelfService(
                    selfContext,
                    timeProvider);
                var current = await selfService.GetCurrentAsync(identity);
                Assert.True(current.Succeeded);
                var draft = current.Intake!;
                var save = await selfService.SaveAsync(
                    identity,
                    BuildSaveCommand(
                        draft,
                        reasonForVisit: "Dolor dental reportado por el paciente"),
                    "intake-self-save");

                Assert.True(save.Succeeded);
                Assert.True(save.Changed);
                Assert.Equal(1, save.Intake!.CurrentRevisionNumber);
            }

            await using (var canonicalCheck = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                Assert.Empty(await canonicalCheck.Patients.ToListAsync());
                Assert.Empty(await canonicalCheck.ClinicalRecords.ToListAsync());
                var intake = await canonicalCheck.PatientIntakes
                    .Include(item => item.Revisions)
                    .SingleAsync();
                Assert.Equal(1, intake.Revisions.Count);
                Assert.Equal(
                    "Dolor dental reportado por el paciente",
                    intake.ReasonForVisit);
            }

            await using (var logoutContext = CreateContext(
                             databaseName,
                             tenantContext))
            {
                var sessionService = new PatientIntakeSessionService(
                    new EfPatientIntakeAuthenticationRepository(logoutContext),
                    timeProvider);
                Assert.True(await sessionService.RevokeCurrentSessionsAsync(
                    identity,
                    "intake-logout"));
            }

            await using (var invalidatedContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var validator = new PatientIntakeSessionValidator(
                    new EfPatientIntakeAuthenticationRepository(invalidatedContext),
                    timeProvider);
                Assert.False(await validator.ValidateAsync(identity));
                var account = await invalidatedContext.PatientPortalAccounts.SingleAsync();
                Assert.Equal(identity.SessionVersion + 1, account.SessionVersion);
                Assert.Contains(
                    await invalidatedContext.PatientIntakeAuthenticationAuditEntries.ToListAsync(),
                    entry => entry.Action == PatientIntakeAuthenticationAuditAction.SessionsRevoked);
            }
        }

        [Fact]
        public async Task LoginAsync_LocksOnlyTheUnlinkedAccountInsideSelectedTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();
            var tenantA = await SeedWaitingRoomLinkAsync(
                databaseName,
                tokenService,
                InitialUtc.AddMinutes(30),
                tenantName: "Tenant A",
                subdomain: "tenant-a");
            var tenantB = await SeedWaitingRoomLinkAsync(
                databaseName,
                tokenService,
                InitialUtc.AddMinutes(30),
                tenantName: "Tenant B",
                subdomain: "tenant-b");
            await ActivateAsync(
                databaseName,
                tenantA.RawToken,
                timeProvider,
                tokenService,
                "shared.login");
            await ActivateAsync(
                databaseName,
                tenantB.RawToken,
                timeProvider,
                tokenService,
                "shared.login");

            for (var attempt = 1; attempt <= 5; attempt++)
            {
                await using var context = CreateContext(
                    databaseName,
                    new TenantContext());
                var result = await CreatePublicService(
                        context,
                        timeProvider,
                        tokenService)
                    .LoginAsync(
                        new LoginPatientIntakeAccountCommand(
                            "tenant-a",
                            "shared.login",
                            "An incorrect patient password."),
                        $"intake-login-failed-{attempt}");
                Assert.Null(result);
                timeProvider.Advance(TimeSpan.FromSeconds(1));
            }

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            var accountA = await verificationContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .SingleAsync(account => account.TenantId == tenantA.Tenant.Id);
            var accountB = await verificationContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .SingleAsync(account => account.TenantId == tenantB.Tenant.Id);
            var auditsA = await verificationContext.PatientIntakeAuthenticationAuditEntries
                .IgnoreQueryFilters()
                .Where(entry => entry.TenantId == tenantA.Tenant.Id)
                .ToListAsync();

            Assert.Equal(5, accountA.FailedLoginAttempts);
            Assert.NotNull(accountA.LockoutEndUtc);
            Assert.Equal(0, accountB.FailedLoginAttempts);
            Assert.Equal(
                5,
                auditsA.Count(entry => entry.Action == PatientIntakeAuthenticationAuditAction.LoginFailed));
            Assert.Single(auditsA.Where(
                entry => entry.Action == PatientIntakeAuthenticationAuditAction.AccountLocked));
        }

        private static async Task<PatientIntakeAuthenticationResponseDto> ActivateAsync(
            string databaseName,
            string rawToken,
            MutableTimeProvider timeProvider,
            PatientIntakeAccessLinkTokenService tokenService,
            string loginName)
        {
            await using var context = CreateContext(
                databaseName,
                new TenantContext());
            var result = await CreatePublicService(
                    context,
                    timeProvider,
                    tokenService)
                .ActivateAsync(
                    new ActivatePatientIntakeAccountCommand(
                        rawToken,
                        loginName,
                        "A sufficiently long waiting-room password."),
                    $"activate-{loginName}");
            Assert.True(result.Succeeded);
            return result.Authentication!;
        }

        private static PatientIntakePublicAuthenticationService CreatePublicService(
            AppDbContext context,
            TimeProvider timeProvider,
            PatientIntakeAccessLinkTokenService tokenService)
        {
            return new PatientIntakePublicAuthenticationService(
                new EfPatientIntakeAuthenticationRepository(context),
                tokenService,
                new PatientPortalPasswordHasher(AuthenticationSettings),
                AuthenticationSettings,
                new PatientPortalJwtTokenService(JwtSettings),
                DraftSettings,
                timeProvider);
        }

        private static PatientIntakeSelfService CreateSelfService(
            AppDbContext context,
            TimeProvider timeProvider)
        {
            return new PatientIntakeSelfService(
                new EfPatientPortalAuthenticationRepository(context),
                new EfPatientIntakeAuthenticationRepository(context),
                new EfPatientIntakeRepository(context),
                DraftSettings,
                timeProvider);
        }

        private static SavePatientIntakeDraftCommand BuildSaveCommand(
            PatientIntakeDto intake,
            string reasonForVisit)
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
                reasonForVisit,
                intake.MedicalAnswers.Select(answer =>
                    new SavePatientIntakeMedicalAnswerCommand(
                        answer.QuestionKey,
                        Enum.Parse<ClinicalMedicalAnswerValue>(answer.Answer),
                        answer.Details)).ToArray(),
                intake.ConcurrencyToken);
        }

        private static async Task<WaitingRoomSeed> SeedWaitingRoomLinkAsync(
            string databaseName,
            PatientIntakeAccessLinkTokenService tokenService,
            DateTime expiresAtUtc,
            DateTime? revokeAtUtc = null,
            string tenantName = "Tenant A",
            string subdomain = "tenant-a")
        {
            await using var context = CreateContext(
                databaseName,
                new TenantContext());
            var tenant = new Tenant(tenantName, subdomain);
            var branch = tenant.AddBranch($"{tenantName} Main");
            var generated = tokenService.Generate();
            var link = new PatientIntakeAccessLink(
                tenant.Id,
                branch,
                PatientIntakeAccessLinkPurpose.NewPatientWaitingRoomRegistration,
                generated.TokenHash,
                InitialUtc,
                expiresAtUtc,
                Guid.NewGuid());
            if (revokeAtUtc.HasValue)
            {
                link.Revoke(Guid.NewGuid(), revokeAtUtc.Value);
            }

            context.Tenants.Add(tenant);
            context.PatientIntakeAccessLinks.Add(link);
            await context.SaveChangesAsync();

            return new WaitingRoomSeed(
                tenant,
                branch,
                link,
                generated.RawToken);
        }

        private static TenantContext CreateIntakeContext(
            PatientIntakeSessionIdentity identity)
        {
            var context = new TenantContext();
            context.SetRequestContext(
                identity.AccountId.ToString(),
                AccessScope.PatientIntake,
                isAuthenticated: true,
                identity.TenantId.ToString());
            return context;
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

        private sealed record WaitingRoomSeed(
            Tenant Tenant,
            Branch Branch,
            PatientIntakeAccessLink Link,
            string RawToken);

        private sealed class FixedAuthenticationSettings
            : IPatientPortalAuthenticationSettings
        {
            public int PasswordHashIterationCount => 100_000;
            public int MinimumPasswordLength => 12;
            public int MaximumPasswordLength => 128;
            public int MaximumFailedLoginAttempts => 5;
            public TimeSpan LockoutDuration => TimeSpan.FromMinutes(15);
        }

        private sealed class FixedJwtSettings : IPatientPortalJwtSettings
        {
            public string Secret =>
                "patient-intake-integration-tests-secret-that-is-long";
            public string Issuer => "BigSmile.PatientIntake.IntegrationTests";
            public string Audience => "BigSmile.PatientIntake.IntegrationTests";
            public TimeSpan AccessTokenLifetime => TimeSpan.FromHours(1);
        }

        private sealed class FixedDraftSettings : IPatientIntakeDraftSettings
        {
            public TimeSpan DraftLifetime => TimeSpan.FromDays(30);
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

            public void Set(DateTime utcNow)
            {
                _utcNow = new DateTimeOffset(utcNow);
            }
        }
    }
}
