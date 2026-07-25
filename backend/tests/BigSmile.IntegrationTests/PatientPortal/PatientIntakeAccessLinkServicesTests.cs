using BigSmile.Application.Features.PatientIntakeAccessLinks.Commands;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Queries;
using BigSmile.Application.Interfaces.PatientIntakes;
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
    public sealed class PatientIntakeAccessLinkServicesTests
    {
        private static readonly DateTime InitialUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task IssueListAndRevoke_PersistOnlyHashAndAppendOnlyAudit()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedTenantAsync(databaseName, "Tenant A", "tenant-a");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var actorUserId = Guid.NewGuid();
            var tenantContext = CreateTenantContext(actorUserId, seed.Tenant.Id);
            var tokenService = new PatientIntakeAccessLinkTokenService(
                new PatientPortalInvitationTokenService());

            string firstRawToken;
            Guid firstAccessLinkId;
            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var service = CreateCommandService(
                    context,
                    tenantContext,
                    timeProvider,
                    tokenService);

                var first = await service.IssueAsync(
                    seed.Branch.Id,
                    "issue-1");
                var second = await service.IssueAsync(
                    seed.Branch.Id,
                    "issue-2");

                Assert.NotNull(first);
                Assert.NotNull(second);
                Assert.NotEqual(first!.BootstrapToken, second!.BootstrapToken);
                Assert.Equal(InitialUtc.AddMinutes(30), first.ExpiresAtUtc);
                Assert.Equal(seed.Branch.Id, first.BranchId);
                firstRawToken = first.BootstrapToken;
                firstAccessLinkId = first.AccessLinkId;
            }

            await using (var verificationContext = CreateContext(
                             databaseName,
                             CreateTenantContext(actorUserId, seed.Tenant.Id)))
            {
                var accessLinks = await verificationContext.PatientIntakeAccessLinks
                    .OrderBy(link => link.CreatedAtUtc)
                    .ThenBy(link => link.Id)
                    .ToListAsync();
                var audits = await verificationContext.PatientIntakeAccessLinkAuditEntries
                    .OrderBy(entry => entry.OccurredAtUtc)
                    .ThenBy(entry => entry.Id)
                    .ToListAsync();

                Assert.Equal(2, accessLinks.Count);
                Assert.Equal(2, audits.Count);
                Assert.All(audits, audit =>
                    Assert.Equal(PatientIntakeAccessLinkAuditAction.Issued, audit.Action));
                Assert.DoesNotContain(accessLinks, link => link.TokenHash == firstRawToken);
                Assert.Contains(accessLinks, link =>
                    link.TokenHash == tokenService.ComputeHash(firstRawToken));
                Assert.DoesNotContain(
                    typeof(PatientIntakeAccessLinkAuditEntry).GetProperties(),
                    property => property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase));
            }

            timeProvider.Advance(TimeSpan.FromMinutes(1));
            await using (var revokeContext = CreateContext(
                             databaseName,
                             CreateTenantContext(actorUserId, seed.Tenant.Id)))
            {
                var commandService = CreateCommandService(
                    revokeContext,
                    CreateTenantContext(actorUserId, seed.Tenant.Id),
                    timeProvider,
                    tokenService);

                Assert.True(await commandService.RevokeAsync(
                    firstAccessLinkId,
                    "revoke-1"));
            }

            await using (var listContext = CreateContext(
                             databaseName,
                             CreateTenantContext(actorUserId, seed.Tenant.Id)))
            {
                var queryService = new PatientIntakeAccessLinkQueryService(
                    new EfPatientIntakeAccessLinkRepository(listContext),
                    CreateTenantContext(actorUserId, seed.Tenant.Id),
                    timeProvider);
                var summaries = await queryService.ListAsync();

                Assert.Equal(2, summaries.Count);
                Assert.Contains(summaries, summary =>
                    summary.AccessLinkId == firstAccessLinkId &&
                    summary.Status == "Revoked" &&
                    !summary.CanRevoke);
                Assert.Contains(summaries, summary =>
                    summary.Status == "Active" && summary.CanRevoke);
            }

            await using (var auditContext = CreateContext(
                             databaseName,
                             CreateTenantContext(actorUserId, seed.Tenant.Id)))
            {
                var audit = await auditContext.PatientIntakeAccessLinkAuditEntries
                    .FirstAsync();
                auditContext.Entry(audit)
                    .Property(nameof(PatientIntakeAccessLinkAuditEntry.CorrelationId))
                    .CurrentValue = "tampered";

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => auditContext.SaveChangesAsync());
                Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task QueryFiltersAndWriteEnforcement_BlockCrossTenantAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedTenantAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedTenantAsync(databaseName, "Tenant B", "tenant-b");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService(
                new PatientPortalInvitationTokenService());
            var actorA = Guid.NewGuid();
            var actorB = Guid.NewGuid();

            Guid accessLinkAId;
            Guid accessLinkBId;
            await using (var contextA = CreateContext(
                             databaseName,
                             CreateTenantContext(actorA, tenantA.Tenant.Id)))
            {
                var tenantContext = CreateTenantContext(actorA, tenantA.Tenant.Id);
                var service = CreateCommandService(
                    contextA,
                    tenantContext,
                    timeProvider,
                    tokenService);
                accessLinkAId = (await service.IssueAsync(null, "tenant-a"))!.AccessLinkId;
            }

            await using (var contextB = CreateContext(
                             databaseName,
                             CreateTenantContext(actorB, tenantB.Tenant.Id)))
            {
                var tenantContext = CreateTenantContext(actorB, tenantB.Tenant.Id);
                var service = CreateCommandService(
                    contextB,
                    tenantContext,
                    timeProvider,
                    tokenService);
                accessLinkBId = (await service.IssueAsync(null, "tenant-b"))!.AccessLinkId;
            }

            await using (var filteredA = CreateContext(
                             databaseName,
                             CreateTenantContext(actorA, tenantA.Tenant.Id)))
            {
                var visible = Assert.Single(await filteredA.PatientIntakeAccessLinks.ToListAsync());
                Assert.Equal(accessLinkAId, visible.Id);
                Assert.Null(await new EfPatientIntakeAccessLinkRepository(filteredA)
                    .GetByIdAsync(accessLinkBId, trackChanges: false));
            }

            var tenantAContext = CreateTenantContext(actorA, tenantA.Tenant.Id);
            await using (var blockedWrite = CreateContext(databaseName, tenantAContext))
            {
                var foreignTenant = await blockedWrite.Tenants
                    .IgnoreQueryFilters()
                    .SingleAsync(tenant => tenant.Id == tenantB.Tenant.Id);
                var foreignLink = new PatientIntakeAccessLink(
                    foreignTenant,
                    branch: null,
                    new string('b', 64),
                    InitialUtc,
                    InitialUtc.AddMinutes(30),
                    actorB);
                blockedWrite.PatientIntakeAccessLinks.Add(foreignLink);

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => blockedWrite.SaveChangesAsync());
                Assert.Contains(
                    "target tenant does not match",
                    exception.Message,
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task Issue_RejectsPlatformOverrideAndForeignOrInactiveBranch()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedTenantAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedTenantAsync(databaseName, "Tenant B", "tenant-b");
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService(
                new PatientPortalInvitationTokenService());
            var actor = Guid.NewGuid();

            var platformContext = new TenantContext();
            platformContext.SetRequestContext(
                actor.ToString(),
                AccessScope.Platform,
                isAuthenticated: true);
            await using (var context = CreateContext(databaseName, platformContext))
            {
                var service = CreateCommandService(
                    context,
                    platformContext,
                    timeProvider,
                    tokenService);
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.IssueAsync(null, "platform"));
            }

            var overrideContext = CreateTenantContext(actor, tenantA.Tenant.Id);
            overrideContext.EnablePlatformOverride();
            await using (var context = CreateContext(databaseName, overrideContext))
            {
                var service = CreateCommandService(
                    context,
                    overrideContext,
                    timeProvider,
                    tokenService);
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.IssueAsync(null, "override"));
            }

            var tenantAContext = CreateTenantContext(actor, tenantA.Tenant.Id);
            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                var service = CreateCommandService(
                    context,
                    tenantAContext,
                    timeProvider,
                    tokenService);
                Assert.Null(await service.IssueAsync(
                    tenantB.Branch.Id,
                    "foreign-branch"));
            }

            await using (var seedContext = CreateContext(databaseName, new TenantContext()))
            {
                var branch = await seedContext.Branches
                    .SingleAsync(candidate => candidate.Id == tenantA.Branch.Id);
                branch.Deactivate();
                await seedContext.SaveChangesAsync();
            }

            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                var service = CreateCommandService(
                    context,
                    tenantAContext,
                    timeProvider,
                    tokenService);
                Assert.Null(await service.IssueAsync(
                    tenantA.Branch.Id,
                    "inactive-branch"));
            }
        }

        [Fact]
        public void Model_ContainsTenantFiltersConcurrencyIndexesAndChecks()
        {
            using var context = CreateContext(
                Guid.NewGuid().ToString(),
                new TenantContext());
            var linkType = context.Model.FindEntityType(typeof(PatientIntakeAccessLink))
                ?? throw new InvalidOperationException("PatientIntakeAccessLink metadata was not found.");
            var auditType = context.Model.FindEntityType(typeof(PatientIntakeAccessLinkAuditEntry))
                ?? throw new InvalidOperationException("PatientIntakeAccessLinkAuditEntry metadata was not found.");

            var tokenIndex = linkType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[] { nameof(PatientIntakeAccessLink.TokenHash) }));
            Assert.True(tokenIndex.IsUnique);
            Assert.True(linkType
                .FindProperty(nameof(PatientIntakeAccessLink.RowVersion))!
                .IsConcurrencyToken);
            Assert.NotNull(linkType.GetQueryFilter());
            Assert.NotNull(auditType.GetQueryFilter());

            var checks = linkType.GetCheckConstraints()
                .Select(constraint => constraint.Name)
                .ToHashSet(StringComparer.Ordinal);
            Assert.Contains("CK_PatientIntakeAccessLinks_ExpiryOrder", checks);
            Assert.Contains("CK_PatientIntakeAccessLinks_RevocationState", checks);
            Assert.Contains("CK_PatientIntakeAccessLinks_ConsumptionState", checks);
            Assert.Contains("CK_PatientIntakeAccessLinks_TerminalState", checks);

            Assert.DoesNotContain(linkType.GetIndexes(), index =>
                index.IsUnique &&
                index.Properties.Any(property => property.Name == nameof(PatientIntakeAccessLink.TenantId)) &&
                index.Properties.Any(property => property.Name == nameof(PatientIntakeAccessLink.BranchId)));
        }

        private static PatientIntakeAccessLinkCommandService CreateCommandService(
            AppDbContext context,
            TenantContext tenantContext,
            TimeProvider timeProvider,
            PatientIntakeAccessLinkTokenService tokenService)
        {
            return new PatientIntakeAccessLinkCommandService(
                new EfPatientIntakeAccessLinkRepository(context),
                new EfTenantRepository(context),
                new EfBranchRepository(context),
                tokenService,
                new FixedAccessLinkSettings(TimeSpan.FromMinutes(30)),
                tenantContext,
                timeProvider);
        }

        private static async Task<SeedData> SeedTenantAsync(
            string databaseName,
            string name,
            string subdomain)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenant = new Tenant(name, subdomain);
            var branch = tenant.AddBranch("Main");
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
            return new SeedData(tenant, branch);
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var context = new TenantContext();
            context.SetRequestContext(
                userId.ToString(),
                AccessScope.Tenant,
                isAuthenticated: true,
                tenantId.ToString());
            return context;
        }

        private static AppDbContext CreateContext(
            string databaseName,
            TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            return new AppDbContext(
                options,
                new ConfigurationBuilder().Build(),
                tenantContext);
        }

        private sealed record SeedData(Tenant Tenant, Branch Branch);

        private sealed class FixedAccessLinkSettings : IPatientIntakeAccessLinkSettings
        {
            public FixedAccessLinkSettings(TimeSpan lifetime)
            {
                WaitingRoomLinkLifetime = lifetime;
            }

            public TimeSpan WaitingRoomLinkLifetime { get; }
        }

        private sealed class MutableTimeProvider : TimeProvider
        {
            private DateTimeOffset _utcNow;

            public MutableTimeProvider(DateTime utcNow)
            {
                _utcNow = new DateTimeOffset(utcNow);
            }

            public override DateTimeOffset GetUtcNow() => _utcNow;

            public void Advance(TimeSpan duration)
            {
                _utcNow = _utcNow.Add(duration);
            }
        }
    }
}
