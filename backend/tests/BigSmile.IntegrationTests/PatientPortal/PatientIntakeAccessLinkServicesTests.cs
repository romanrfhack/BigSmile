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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
            var actorUserId = Guid.NewGuid();
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = CreateTokenService();

            string rawToken;
            Guid firstLinkId;
            var staffContext = CreateTenantContext(actorUserId, seed.Tenant.Id);
            await using (var context = CreateContext(databaseName, staffContext))
            {
                var service = CreateCommandService(
                    context,
                    staffContext,
                    timeProvider,
                    tokenService);
                var first = await service.IssueAsync(seed.Branch.Id, "issue-1");
                var second = await service.IssueAsync(seed.Branch.Id, "issue-2");

                Assert.NotNull(first);
                Assert.NotNull(second);
                Assert.NotEqual(first!.BootstrapToken, second!.BootstrapToken);
                Assert.Equal(seed.Branch.Id, first.BranchId);
                Assert.Equal(InitialUtc.AddMinutes(30), first.ExpiresAtUtc);
                rawToken = first.BootstrapToken;
                firstLinkId = first.AccessLinkId;
            }

            await using (var verificationContext = CreateContext(
                             databaseName,
                             CreateTenantContext(actorUserId, seed.Tenant.Id)))
            {
                var links = await verificationContext.PatientIntakeAccessLinks
                    .OrderBy(link => link.Id)
                    .ToListAsync();
                var audits = await verificationContext.PatientIntakeAccessLinkAuditEntries
                    .OrderBy(entry => entry.Id)
                    .ToListAsync();

                Assert.Equal(2, links.Count);
                Assert.Equal(2, audits.Count);
                Assert.All(audits, audit =>
                    Assert.Equal(PatientIntakeAccessLinkAuditAction.Issued, audit.Action));
                Assert.DoesNotContain(links, link => link.TokenHash == rawToken);
                Assert.Contains(links, link =>
                    link.TokenHash == tokenService.ComputeHash(rawToken));
                Assert.DoesNotContain(
                    typeof(PatientIntakeAccessLinkAuditEntry).GetProperties(),
                    property => property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase));
            }

            timeProvider.Advance(TimeSpan.FromMinutes(1));
            var revokeContextValue = CreateTenantContext(actorUserId, seed.Tenant.Id);
            await using (var revokeContext = CreateContext(databaseName, revokeContextValue))
            {
                var service = CreateCommandService(
                    revokeContext,
                    revokeContextValue,
                    timeProvider,
                    tokenService);
                Assert.True(await service.RevokeAsync(firstLinkId, "revoke-1"));
            }

            var listContextValue = CreateTenantContext(actorUserId, seed.Tenant.Id);
            await using (var listContext = CreateContext(databaseName, listContextValue))
            {
                var query = new PatientIntakeAccessLinkQueryService(
                    new EfPatientIntakeAccessLinkRepository(listContext),
                    listContextValue,
                    timeProvider);
                var summaries = await query.ListAsync();

                Assert.Equal(2, summaries.Count);
                Assert.Contains(summaries, summary =>
                    summary.AccessLinkId == firstLinkId &&
                    summary.Status == "Revoked" &&
                    !summary.CanRevoke);
                Assert.Contains(summaries, summary =>
                    summary.Status == "Active" && summary.CanRevoke);
            }

            await using (var auditContext = CreateContext(
                             databaseName,
                             CreateTenantContext(actorUserId, seed.Tenant.Id)))
            {
                var audit = await auditContext.PatientIntakeAccessLinkAuditEntries.FirstAsync();
                auditContext.Entry(audit)
                    .Property(nameof(PatientIntakeAccessLinkAuditEntry.CorrelationId))
                    .CurrentValue = "tampered";

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => auditContext.SaveChangesAsync());
                Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public async Task TenantFiltersWriteEnforcementAndPrivilegeGuards_BlockUnsafeAccess()
        {
            var databaseName = Guid.NewGuid().ToString();
            var tenantA = await SeedTenantAsync(databaseName, "Tenant A", "tenant-a");
            var tenantB = await SeedTenantAsync(databaseName, "Tenant B", "tenant-b");
            var actorA = Guid.NewGuid();
            var actorB = Guid.NewGuid();
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = CreateTokenService();

            Guid linkAId;
            Guid linkBId;
            var contextAValue = CreateTenantContext(actorA, tenantA.Tenant.Id);
            await using (var contextA = CreateContext(databaseName, contextAValue))
            {
                linkAId = (await CreateCommandService(
                    contextA,
                    contextAValue,
                    timeProvider,
                    tokenService).IssueAsync(null, "tenant-a"))!.AccessLinkId;
            }

            var contextBValue = CreateTenantContext(actorB, tenantB.Tenant.Id);
            await using (var contextB = CreateContext(databaseName, contextBValue))
            {
                linkBId = (await CreateCommandService(
                    contextB,
                    contextBValue,
                    timeProvider,
                    tokenService).IssueAsync(null, "tenant-b"))!.AccessLinkId;
            }

            await using (var filteredA = CreateContext(
                             databaseName,
                             CreateTenantContext(actorA, tenantA.Tenant.Id)))
            {
                var visible = Assert.Single(await filteredA.PatientIntakeAccessLinks.ToListAsync());
                Assert.Equal(linkAId, visible.Id);
                Assert.Null(await new EfPatientIntakeAccessLinkRepository(filteredA)
                    .GetByIdAsync(linkBId, trackChanges: false));
            }

            var tenantAWriteContext = CreateTenantContext(actorA, tenantA.Tenant.Id);
            await using (var blockedWrite = CreateContext(databaseName, tenantAWriteContext))
            {
                var foreignTenant = await blockedWrite.Tenants
                    .IgnoreQueryFilters()
                    .SingleAsync(tenant => tenant.Id == tenantB.Tenant.Id);
                blockedWrite.PatientIntakeAccessLinks.Add(new PatientIntakeAccessLink(
                    foreignTenant,
                    branch: null,
                    new string('b', 64),
                    InitialUtc,
                    InitialUtc.AddMinutes(30),
                    actorB));

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => blockedWrite.SaveChangesAsync());
                Assert.Contains(
                    "target tenant does not match",
                    exception.Message,
                    StringComparison.OrdinalIgnoreCase);
            }

            var platformContext = new TenantContext();
            platformContext.SetRequestContext(
                actorA.ToString(),
                AccessScope.Platform,
                isAuthenticated: true);
            await using (var platformDb = CreateContext(databaseName, platformContext))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    CreateCommandService(
                        platformDb,
                        platformContext,
                        timeProvider,
                        tokenService).IssueAsync(null, "platform"));
            }

            var overrideContext = CreateTenantContext(actorA, tenantA.Tenant.Id);
            overrideContext.EnablePlatformOverride();
            await using (var overrideDb = CreateContext(databaseName, overrideContext))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    CreateCommandService(
                        overrideDb,
                        overrideContext,
                        timeProvider,
                        tokenService).IssueAsync(null, "override"));
            }

            var branchContext = CreateTenantContext(actorA, tenantA.Tenant.Id);
            await using (var branchDb = CreateContext(databaseName, branchContext))
            {
                var service = CreateCommandService(
                    branchDb,
                    branchContext,
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

            var inactiveContext = CreateTenantContext(actorA, tenantA.Tenant.Id);
            await using (var inactiveDb = CreateContext(databaseName, inactiveContext))
            {
                Assert.Null(await CreateCommandService(
                    inactiveDb,
                    inactiveContext,
                    timeProvider,
                    tokenService).IssueAsync(
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
            var model = context.GetService<IDesignTimeModel>().Model;
            var linkType = model.FindEntityType(typeof(PatientIntakeAccessLink))
                ?? throw new InvalidOperationException("PatientIntakeAccessLink metadata was not found.");
            var auditType = model.FindEntityType(typeof(PatientIntakeAccessLinkAuditEntry))
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

        private static PatientIntakeAccessLinkTokenService CreateTokenService()
        {
            return new PatientIntakeAccessLinkTokenService(
                new PatientPortalInvitationTokenService());
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
