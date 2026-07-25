using BigSmile.Application.Features.PatientPortalAuthentication.Commands;
using BigSmile.Application.Features.PatientPortalAuthentication.Dtos;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
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
    public sealed class PatientPortalAuthenticationServicesTests
    {
        private static readonly DateTime InitialUtc = new(2026, 7, 25, 9, 0, 0, DateTimeKind.Utc);
        private static readonly FixedAuthenticationSettings AuthenticationSettings = new();
        private static readonly FixedJwtSettings JwtSettings = new();

        [Fact]
        public async Task ActivateAsync_CreatesAccountConsumesInvitationAndRejectsReplay()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();
            var rawToken = await IssueInvitationAsync(databaseName, seed, timeProvider, tokenService);

            PatientPortalAuthenticationResponseDto authentication;
            await using (var context = CreateContext(databaseName, new TenantContext()))
            {
                var service = CreatePublicService(context, timeProvider, tokenService);
                var result = await service.ActivateAsync(
                    new ActivatePatientPortalAccountCommand(
                        rawToken,
                        "ana.portal",
                        "A sufficiently long patient password."),
                    "activation-1");

                Assert.True(result.Succeeded);
                authentication = Assert.IsType<PatientPortalAuthenticationResponseDto>(result.Authentication);
                Assert.Equal(seed.Patient.Id, authentication.Current.PatientId);
                Assert.Equal("tenant-a", authentication.Current.TenantSubdomain);
            }

            await using (var verificationContext = CreateContext(databaseName, new TenantContext()))
            {
                var account = await verificationContext.PatientPortalAccounts.SingleAsync();
                var invitation = await verificationContext.PatientPortalInvitations.SingleAsync();
                var audits = await verificationContext.PatientPortalAuthenticationAuditEntries
                    .OrderBy(entry => entry.Action)
                    .ToListAsync();

                Assert.True(account.IsActive);
                Assert.Equal("ANA.PORTAL", account.NormalizedLoginName);
                Assert.NotEqual("A sufficiently long patient password.", account.PasswordHash);
                Assert.Equal(account.Id, invitation.ConsumedByPatientPortalAccountId);
                Assert.Equal(InitialUtc, invitation.ConsumedAtUtc);
                Assert.Equal(2, audits.Count);
                Assert.Contains(audits, entry => entry.Action == PatientPortalAuthenticationAuditAction.AccountActivated);
                Assert.Contains(audits, entry => entry.Action == PatientPortalAuthenticationAuditAction.InvitationConsumed);
            }

            await using (var replayContext = CreateContext(databaseName, new TenantContext()))
            {
                var service = CreatePublicService(replayContext, timeProvider, tokenService);
                var replay = await service.ActivateAsync(
                    new ActivatePatientPortalAccountCommand(
                        rawToken,
                        "other.login",
                        "Another sufficiently long password."),
                    "activation-replay");

                Assert.False(replay.Succeeded);
                Assert.Equal(PatientPortalActivationFailure.InvalidActivation, replay.Failure);
            }
        }

        [Fact]
        public async Task LoginAsync_IsTenantScopedAndLocksAtConfiguredThreshold()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedAsync(databaseName, "Tenant B", "tenant-b");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();
            await ActivateAsync(databaseName, tenantA, timeProvider, tokenService, "shared.login");
            await ActivateAsync(databaseName, tenantB, timeProvider, tokenService, "shared.login");

            await using (var wrongRealmContext = CreateContext(databaseName, new TenantContext()))
            {
                var service = CreatePublicService(wrongRealmContext, timeProvider, tokenService);
                Assert.Null(await service.LoginAsync(
                    new LoginPatientPortalAccountCommand(
                        "unknown-tenant",
                        "shared.login",
                        "A sufficiently long patient password."),
                    "wrong-realm"));
            }

            for (var attempt = 1; attempt <= 5; attempt++)
            {
                await using var failureContext = CreateContext(databaseName, new TenantContext());
                var service = CreatePublicService(failureContext, timeProvider, tokenService);
                Assert.Null(await service.LoginAsync(
                    new LoginPatientPortalAccountCommand(
                        "tenant-a",
                        "shared.login",
                        "An incorrect patient password."),
                    $"login-failed-{attempt}"));
                timeProvider.Advance(TimeSpan.FromSeconds(1));
            }

            await using (var lockedContext = CreateContext(databaseName, new TenantContext()))
            {
                var service = CreatePublicService(lockedContext, timeProvider, tokenService);
                Assert.Null(await service.LoginAsync(
                    new LoginPatientPortalAccountCommand(
                        "tenant-a",
                        "shared.login",
                        "A sufficiently long patient password."),
                    "login-while-locked"));
            }

            await using (var verificationContext = CreateContext(databaseName, new TenantContext()))
            {
                var tenantAAccount = await verificationContext.PatientPortalAccounts
                    .IgnoreQueryFilters()
                    .SingleAsync(account => account.TenantId == tenantA.Tenant.Id);
                var tenantBAccount = await verificationContext.PatientPortalAccounts
                    .IgnoreQueryFilters()
                    .SingleAsync(account => account.TenantId == tenantB.Tenant.Id);
                var tenantAAudits = await verificationContext.PatientPortalAuthenticationAuditEntries
                    .IgnoreQueryFilters()
                    .Where(entry => entry.TenantId == tenantA.Tenant.Id)
                    .ToListAsync();

                Assert.Equal(5, tenantAAccount.FailedLoginAttempts);
                Assert.NotNull(tenantAAccount.LockoutEndUtc);
                Assert.Equal(0, tenantBAccount.FailedLoginAttempts);
                Assert.Equal(5, tenantAAudits.Count(entry => entry.Action == PatientPortalAuthenticationAuditAction.LoginFailed));
                Assert.Single(tenantAAudits.Where(entry => entry.Action == PatientPortalAuthenticationAuditAction.AccountLocked));
            }

            timeProvider.Advance(TimeSpan.FromMinutes(16));
            await using (var successfulContext = CreateContext(databaseName, new TenantContext()))
            {
                var service = CreatePublicService(successfulContext, timeProvider, tokenService);
                var result = await service.LoginAsync(
                    new LoginPatientPortalAccountCommand(
                        "tenant-a",
                        "shared.login",
                        "A sufficiently long patient password."),
                    "login-after-lockout");

                Assert.NotNull(result);
                Assert.Equal(tenantA.Patient.Id, result!.Current.PatientId);
            }
        }

        [Fact]
        public async Task SessionValidationAndLogout_InvalidatePriorSessionVersion()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();
            var authentication = await ActivateAsync(databaseName, seed, timeProvider, tokenService, "session.login");
            var identity = new PatientPortalSessionIdentity(
                authentication.Current.AccountId,
                seed.Tenant.Id,
                seed.Patient.Id,
                authentication.Current.SessionVersion);

            await using (var validationContext = CreateContext(databaseName, new TenantContext()))
            {
                var validator = new PatientPortalSessionValidator(
                    new EfPatientPortalAuthenticationRepository(validationContext));
                Assert.True(await validator.ValidateAsync(identity));
            }

            var patientContext = new TenantContext();
            patientContext.SetRequestContext(
                identity.AccountId.ToString(),
                AccessScope.Patient,
                isAuthenticated: true,
                identity.TenantId.ToString());
            await using (var logoutContext = CreateContext(databaseName, patientContext))
            {
                var sessionService = new PatientPortalSessionService(
                    new EfPatientPortalAuthenticationRepository(logoutContext),
                    timeProvider);
                Assert.True(await sessionService.RevokeCurrentSessionsAsync(identity, "logout-1"));
            }

            await using (var verificationContext = CreateContext(databaseName, new TenantContext()))
            {
                var validator = new PatientPortalSessionValidator(
                    new EfPatientPortalAuthenticationRepository(verificationContext));
                Assert.False(await validator.ValidateAsync(identity));
                var account = await verificationContext.PatientPortalAccounts.SingleAsync();
                Assert.Equal(identity.SessionVersion + 1, account.SessionVersion);
                Assert.Contains(
                    await verificationContext.PatientPortalAuthenticationAuditEntries.ToListAsync(),
                    entry => entry.Action == PatientPortalAuthenticationAuditAction.SessionsRevoked);
            }
        }

        [Fact]
        public async Task StartRecoveryAsync_DeactivatesAccountAndIssuesReplacementCredential()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();
            var authentication = await ActivateAsync(databaseName, seed, timeProvider, tokenService, "recovery.login");
            timeProvider.Advance(TimeSpan.FromHours(1));

            var staffContext = CreateStaffContext(seed.User.Id, seed.Tenant.Id);
            string recoveryToken;
            await using (var recoveryContext = CreateContext(databaseName, staffContext))
            {
                var service = new PatientPortalRecoveryService(
                    new EfPatientRepository(recoveryContext),
                    new EfPatientPortalAuthenticationRepository(recoveryContext),
                    new EfPatientPortalInvitationRepository(recoveryContext),
                    tokenService,
                    new FixedInvitationSettings(TimeSpan.FromHours(24)),
                    staffContext,
                    timeProvider);
                var result = await service.StartRecoveryAsync(seed.Patient.Id, "recovery-start");

                Assert.NotNull(result);
                recoveryToken = result!.ActivationToken;
            }

            await using (var verificationContext = CreateContext(databaseName, new TenantContext()))
            {
                var account = await verificationContext.PatientPortalAccounts.SingleAsync();
                var invitations = await verificationContext.PatientPortalInvitations
                    .OrderBy(invitation => invitation.CreatedAtUtc)
                    .ToListAsync();

                Assert.False(account.IsActive);
                Assert.True(account.SessionVersion > authentication.Current.SessionVersion);
                Assert.Equal(2, invitations.Count);
                Assert.NotNull(invitations[0].ConsumedAtUtc);
                Assert.True(invitations[1].CanBeConsumedAt(timeProvider.GetUtcNow().UtcDateTime));
                Assert.Contains(
                    await verificationContext.PatientPortalAuthenticationAuditEntries.ToListAsync(),
                    entry => entry.Action == PatientPortalAuthenticationAuditAction.RecoveryStarted &&
                             entry.ActorType == PatientPortalAuthenticationAuditActorType.StaffUser);
            }

            await using (var activationContext = CreateContext(databaseName, new TenantContext()))
            {
                var service = CreatePublicService(activationContext, timeProvider, tokenService);
                var recovered = await service.ActivateAsync(
                    new ActivatePatientPortalAccountCommand(
                        recoveryToken,
                        "recovery.login",
                        "A replacement patient password value."),
                    "recovery-complete");

                Assert.True(recovered.Succeeded);
                Assert.Equal("recovery.login", recovered.Authentication!.Current.LoginName);
                Assert.True(recovered.Authentication.Current.SessionVersion > authentication.Current.SessionVersion);
            }
        }

        [Fact]
        public async Task AuthenticationAuditEntries_AreTenantFilteredAndAppendOnly()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedAsync(databaseName, "Tenant B", "tenant-b");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();
            await ActivateAsync(databaseName, tenantA, timeProvider, tokenService, "tenant-a.login");
            await ActivateAsync(databaseName, tenantB, timeProvider, tokenService, "tenant-b.login");

            var tenantAContext = CreateStaffContext(tenantA.User.Id, tenantA.Tenant.Id);
            await using var filteredContext = CreateContext(databaseName, tenantAContext);
            var entries = await filteredContext.PatientPortalAuthenticationAuditEntries.ToListAsync();

            Assert.NotEmpty(entries);
            Assert.All(entries, entry => Assert.Equal(tenantA.Tenant.Id, entry.TenantId));

            var entry = entries[0];
            filteredContext.Entry(entry).Property(nameof(PatientPortalAuthenticationAuditEntry.CorrelationId)).CurrentValue = "changed";
            filteredContext.Entry(entry).State = EntityState.Modified;
            await Assert.ThrowsAsync<InvalidOperationException>(() => filteredContext.SaveChangesAsync());
        }

        private static async Task<PatientPortalAuthenticationResponseDto> ActivateAsync(
            string databaseName,
            SeedData seed,
            MutableTimeProvider timeProvider,
            PatientPortalInvitationTokenService tokenService,
            string loginName)
        {
            var rawToken = await IssueInvitationAsync(databaseName, seed, timeProvider, tokenService);
            await using var context = CreateContext(databaseName, new TenantContext());
            var service = CreatePublicService(context, timeProvider, tokenService);
            var result = await service.ActivateAsync(
                new ActivatePatientPortalAccountCommand(
                    rawToken,
                    loginName,
                    "A sufficiently long patient password."),
                $"activate-{loginName}");

            Assert.True(result.Succeeded);
            return result.Authentication!;
        }

        private static async Task<string> IssueInvitationAsync(
            string databaseName,
            SeedData seed,
            MutableTimeProvider timeProvider,
            PatientPortalInvitationTokenService tokenService)
        {
            var staffContext = CreateStaffContext(seed.User.Id, seed.Tenant.Id);
            await using var context = CreateContext(databaseName, staffContext);
            var service = new PatientPortalInvitationCommandService(
                new EfPatientPortalInvitationRepository(context),
                new EfPatientRepository(context),
                tokenService,
                new FixedInvitationSettings(TimeSpan.FromHours(24)),
                staffContext,
                timeProvider);
            var result = await service.IssueAsync(seed.Patient.Id, $"issue-{seed.Patient.Id:N}");
            return result!.ActivationToken;
        }

        private static PatientPortalPublicAuthenticationService CreatePublicService(
            AppDbContext context,
            TimeProvider timeProvider,
            PatientPortalInvitationTokenService tokenService)
        {
            var passwordHasher = new PatientPortalPasswordHasher(AuthenticationSettings);
            return new PatientPortalPublicAuthenticationService(
                new EfPatientPortalAuthenticationRepository(context),
                tokenService,
                passwordHasher,
                AuthenticationSettings,
                new PatientPortalJwtTokenService(JwtSettings),
                timeProvider);
        }

        private static TenantContext CreateStaffContext(Guid userId, Guid tenantId)
        {
            var context = new TenantContext();
            context.SetRequestContext(
                userId.ToString(),
                AccessScope.Tenant,
                isAuthenticated: true,
                tenantId.ToString());
            return context;
        }

        private static async Task<SeedData> SeedAsync(
            string databaseName,
            string tenantName,
            string subdomain)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenant = new Tenant(tenantName, subdomain);
            var patient = new Patient(
                tenant.Id,
                tenantName.Replace("Tenant ", string.Empty),
                "Patient",
                new DateOnly(1991, 2, 14));
            var user = new User($"{subdomain}@example.com", "hashed-password", $"{tenantName} Admin");

            context.Tenants.Add(tenant);
            context.Patients.Add(patient);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return new SeedData(tenant, patient, user);
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var configuration = new ConfigurationBuilder().Build();
            return new AppDbContext(options, configuration, tenantContext);
        }

        private sealed record SeedData(Tenant Tenant, Patient Patient, User User);

        private sealed class FixedAuthenticationSettings : IPatientPortalAuthenticationSettings
        {
            public int PasswordHashIterationCount => 100_000;
            public int MinimumPasswordLength => 12;
            public int MaximumPasswordLength => 128;
            public int MaximumFailedLoginAttempts => 5;
            public TimeSpan LockoutDuration => TimeSpan.FromMinutes(15);
        }

        private sealed class FixedJwtSettings : IPatientPortalJwtSettings
        {
            public string Secret => "patient-portal-test-secret-that-is-distinct-and-long-enough";
            public string Issuer => "BigSmile.PatientPortal.Tests";
            public string Audience => "BigSmile.PatientPortal.Tests";
            public TimeSpan AccessTokenLifetime => TimeSpan.FromHours(1);
        }

        private sealed class FixedInvitationSettings : IPatientPortalInvitationSettings
        {
            public FixedInvitationSettings(TimeSpan lifetime)
            {
                ExistingPatientActivationLifetime = lifetime;
            }

            public TimeSpan ExistingPatientActivationLifetime { get; }
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
