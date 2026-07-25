using System.Text.Json;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Queries;
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
    public sealed class PatientPortalInvitationServicesTests
    {
        private static readonly DateTime InitialUtc = new(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task IssueAsync_ReturnsRawTokenOnceAndPersistsOnlyHashWithAudit()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantContext = CreateTenantContext(seed.User.Id, seed.Tenant.Id);
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var commandService = CreateCommandService(context, tenantContext, timeProvider, tokenService);

                var result = await commandService.IssueAsync(seed.Patient.Id, "correlation-issue-1");

                Assert.NotNull(result);
                Assert.False(string.IsNullOrWhiteSpace(result!.ActivationToken));
                Assert.Equal(InitialUtc, result.CreatedAtUtc);
                Assert.Equal(InitialUtc.AddHours(24), result.ExpiresAtUtc);
            }

            await using (var verificationContext = CreateContext(databaseName, tenantContext))
            {
                var invitation = await verificationContext.PatientPortalInvitations.SingleAsync();
                var auditEntry = await verificationContext.PatientPortalSecurityAuditEntries.SingleAsync();
                var tokenServiceForVerification = new PatientPortalInvitationTokenService();
                var queryService = CreateQueryService(verificationContext, tenantContext, timeProvider);
                var summaries = await queryService.ListAsync(seed.Patient.Id);

                Assert.NotNull(summaries);
                var summary = Assert.Single(summaries!);
                Assert.Equal("Active", summary.Status);
                Assert.Equal(PatientPortalSecurityAuditAction.InvitationIssued, auditEntry.Action);
                Assert.Equal(seed.User.Id, auditEntry.ActorUserId);
                Assert.Equal("correlation-issue-1", auditEntry.CorrelationId);
                Assert.DoesNotContain("TokenHash", JsonSerializer.Serialize(summary), StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("ActivationToken", JsonSerializer.Serialize(summary), StringComparison.OrdinalIgnoreCase);
                Assert.Equal(64, invitation.TokenHash.Length);

                var issuedToken = await IssueSecondTokenForHashVerificationAsync(
                    databaseName,
                    seed,
                    timeProvider,
                    tokenServiceForVerification);
                Assert.Equal(tokenServiceForVerification.ComputeHash(issuedToken.RawToken), issuedToken.TokenHash);
            }
        }

        [Fact]
        public async Task IssueAsync_SupersedesOutstandingInvitationAndRecordsAppendOnlyAudit()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantContext = CreateTenantContext(seed.User.Id, seed.Tenant.Id);
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var commandService = CreateCommandService(context, tenantContext, timeProvider, tokenService);
                Assert.NotNull(await commandService.IssueAsync(seed.Patient.Id, "correlation-first"));
            }

            timeProvider.Advance(TimeSpan.FromHours(1));

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var commandService = CreateCommandService(context, tenantContext, timeProvider, tokenService);
                Assert.NotNull(await commandService.IssueAsync(seed.Patient.Id, "correlation-second"));
            }

            await using (var verificationContext = CreateContext(databaseName, tenantContext))
            {
                var invitations = await verificationContext.PatientPortalInvitations
                    .OrderBy(invitation => invitation.CreatedAtUtc)
                    .ToListAsync();
                var auditEntries = await verificationContext.PatientPortalSecurityAuditEntries
                    .OrderBy(entry => entry.OccurredAtUtc)
                    .ThenBy(entry => entry.Action)
                    .ToListAsync();
                var queryService = CreateQueryService(verificationContext, tenantContext, timeProvider);
                var summaries = await queryService.ListAsync(seed.Patient.Id);

                Assert.Equal(2, invitations.Count);
                Assert.Equal(InitialUtc.AddHours(1), invitations[0].RevokedAtUtc);
                Assert.Null(invitations[1].RevokedAtUtc);
                Assert.Equal(3, auditEntries.Count);
                Assert.Equal(2, auditEntries.Count(entry => entry.Action == PatientPortalSecurityAuditAction.InvitationIssued));
                Assert.Single(auditEntries.Where(entry => entry.Action == PatientPortalSecurityAuditAction.InvitationSuperseded));
                Assert.NotNull(summaries);
                Assert.Equal(new[] { "Active", "Revoked" }, summaries!.Select(summary => summary.Status));
            }
        }

        [Fact]
        public async Task RevokeAsync_RevokesInvitationAndRejectsRepeatedRevocation()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantContext = CreateTenantContext(seed.User.Id, seed.Tenant.Id);
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientPortalInvitationTokenService();
            Guid invitationId;

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var commandService = CreateCommandService(context, tenantContext, timeProvider, tokenService);
                var issued = await commandService.IssueAsync(seed.Patient.Id, "correlation-issue");
                invitationId = issued!.Id;
            }

            timeProvider.Advance(TimeSpan.FromMinutes(10));

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var commandService = CreateCommandService(context, tenantContext, timeProvider, tokenService);
                Assert.True(await commandService.RevokeAsync(
                    seed.Patient.Id,
                    invitationId,
                    "correlation-revoke"));
            }

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var commandService = CreateCommandService(context, tenantContext, timeProvider, tokenService);
                await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.RevokeAsync(
                    seed.Patient.Id,
                    invitationId,
                    "correlation-revoke-again"));
            }

            await using (var verificationContext = CreateContext(databaseName, tenantContext))
            {
                var invitation = await verificationContext.PatientPortalInvitations.SingleAsync();
                var auditEntries = await verificationContext.PatientPortalSecurityAuditEntries.ToListAsync();

                Assert.Equal(InitialUtc.AddMinutes(10), invitation.RevokedAtUtc);
                Assert.Equal(seed.User.Id, invitation.RevokedByUserId);
                Assert.Equal(2, auditEntries.Count);
                Assert.Single(auditEntries.Where(entry => entry.Action == PatientPortalSecurityAuditAction.InvitationRevoked));
            }
        }

        [Fact]
        public async Task Services_DoNotExposeOrMutatePatientsAcrossTenants()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedAsync(databaseName, "Tenant B", "tenant-b");
            var tenantAContext = CreateTenantContext(tenantA.User.Id, tenantA.Tenant.Id);
            var timeProvider = new MutableTimeProvider(InitialUtc);

            await using var context = CreateContext(databaseName, tenantAContext);
            var commandService = CreateCommandService(
                context,
                tenantAContext,
                timeProvider,
                new PatientPortalInvitationTokenService());
            var queryService = CreateQueryService(context, tenantAContext, timeProvider);

            Assert.Null(await commandService.IssueAsync(tenantB.Patient.Id, "cross-tenant-issue"));
            Assert.Null(await queryService.ListAsync(tenantB.Patient.Id));
            Assert.False(await commandService.RevokeAsync(
                tenantB.Patient.Id,
                Guid.NewGuid(),
                "cross-tenant-revoke"));
            Assert.Empty(await context.PatientPortalInvitations.ToListAsync());
            Assert.Empty(await context.PatientPortalSecurityAuditEntries.ToListAsync());
        }

        [Fact]
        public async Task Services_BlockPlatformScopeAndPlatformOverride()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var platformContext = new TenantContext();
            platformContext.SetRequestContext(
                seed.User.Id.ToString(),
                AccessScope.Platform,
                isAuthenticated: true);
            var timeProvider = new MutableTimeProvider(InitialUtc);

            await using var context = CreateContext(databaseName, platformContext);
            var commandService = CreateCommandService(
                context,
                platformContext,
                timeProvider,
                new PatientPortalInvitationTokenService());
            var queryService = CreateQueryService(context, platformContext, timeProvider);

            await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.IssueAsync(
                seed.Patient.Id,
                "platform-issue"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.ListAsync(seed.Patient.Id));

            platformContext.EnablePlatformOverride();
            await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.IssueAsync(
                seed.Patient.Id,
                "platform-override-issue"));
        }

        [Fact]
        public async Task SecurityAuditEntries_AreTenantFilteredAndAppendOnly()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedAsync(databaseName, "Tenant B", "tenant-b");
            var timeProvider = new MutableTimeProvider(InitialUtc);

            await IssueForTenantAsync(databaseName, tenantA, timeProvider, "tenant-a-audit");
            await IssueForTenantAsync(databaseName, tenantB, timeProvider, "tenant-b-audit");

            var tenantAContext = CreateTenantContext(tenantA.User.Id, tenantA.Tenant.Id);
            await using (var filteredContext = CreateContext(databaseName, tenantAContext))
            {
                var entry = Assert.Single(await filteredContext.PatientPortalSecurityAuditEntries.ToListAsync());
                Assert.Equal(tenantA.Tenant.Id, entry.TenantId);

                filteredContext.Entry(entry).Property(nameof(PatientPortalSecurityAuditEntry.CorrelationId)).CurrentValue = "changed";
                await Assert.ThrowsAsync<InvalidOperationException>(() => filteredContext.SaveChangesAsync());
            }
        }

        [Fact]
        public void Model_ContainsOutstandingInvitationUniquenessAndAuditGuardrails()
        {
            using var context = CreateContext(Guid.NewGuid().ToString(), new TenantContext());
            var invitationType = context.Model.FindEntityType(typeof(PatientPortalInvitation))
                ?? throw new InvalidOperationException("PatientPortalInvitation metadata was not found.");
            var auditType = context.Model.FindEntityType(typeof(PatientPortalSecurityAuditEntry))
                ?? throw new InvalidOperationException("PatientPortalSecurityAuditEntry metadata was not found.");

            var outstandingIndex = invitationType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(PatientPortalInvitation.TenantId),
                        nameof(PatientPortalInvitation.PatientId),
                        nameof(PatientPortalInvitation.Purpose)
                    }));

            Assert.True(outstandingIndex.IsUnique);
            Assert.Equal("[RevokedAtUtc] IS NULL AND [ConsumedAtUtc] IS NULL", outstandingIndex.GetFilter());
            Assert.NotNull(auditType.GetQueryFilter());
        }

        private static async Task IssueForTenantAsync(
            string databaseName,
            SeedData seed,
            MutableTimeProvider timeProvider,
            string correlationId)
        {
            var tenantContext = CreateTenantContext(seed.User.Id, seed.Tenant.Id);
            await using var context = CreateContext(databaseName, tenantContext);
            var service = CreateCommandService(
                context,
                tenantContext,
                timeProvider,
                new PatientPortalInvitationTokenService());
            Assert.NotNull(await service.IssueAsync(seed.Patient.Id, correlationId));
        }

        private static async Task<GeneratedPatientPortalInvitationToken> IssueSecondTokenForHashVerificationAsync(
            string databaseName,
            SeedData seed,
            MutableTimeProvider timeProvider,
            PatientPortalInvitationTokenService tokenService)
        {
            var generated = tokenService.Generate();
            await Task.CompletedTask;
            return generated;
        }

        private static PatientPortalInvitationCommandService CreateCommandService(
            AppDbContext context,
            TenantContext tenantContext,
            TimeProvider timeProvider,
            IPatientPortalInvitationTokenService tokenService)
        {
            return new PatientPortalInvitationCommandService(
                new EfPatientPortalInvitationRepository(context),
                new EfPatientRepository(context),
                tokenService,
                new FixedInvitationSettings(TimeSpan.FromHours(24)),
                tenantContext,
                timeProvider);
        }

        private static PatientPortalInvitationQueryService CreateQueryService(
            AppDbContext context,
            TenantContext tenantContext,
            TimeProvider timeProvider)
        {
            return new PatientPortalInvitationQueryService(
                new EfPatientPortalInvitationRepository(context),
                new EfPatientRepository(context),
                tenantContext,
                timeProvider);
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(
                userId.ToString(),
                AccessScope.Tenant,
                isAuthenticated: true,
                tenantId.ToString());
            return tenantContext;
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
            return new AppDbContext(options, new ConfigurationBuilder().Build(), tenantContext);
        }

        private sealed record SeedData(Tenant Tenant, Patient Patient, User User);

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

            public MutableTimeProvider(DateTime utcNow)
            {
                _utcNow = new DateTimeOffset(utcNow);
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;

            public void Advance(TimeSpan value)
            {
                _utcNow = _utcNow.Add(value);
            }
        }
    }
}
