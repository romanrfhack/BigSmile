using BigSmile.Application.Features.PatientIntakeAccessLinks.Commands;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos;
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
        public async Task IssueListAndRevoke_AreTenantScopedHashOnlyAndAudited()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedTwoTenantsAsync(databaseName);
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();
            var tenantAContext = CreateStaffContext(seed.UserA.Id, seed.TenantA.Id);

            IssuedPatientIntakeAccessLinkDto issued;
            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                var service = CreateCommandService(
                    context,
                    tenantAContext,
                    timeProvider,
                    tokenService);
                var result = await service.IssueAsync(
                    seed.BranchA.Id,
                    "issue-waiting-room-1");

                Assert.True(result.Succeeded);
                issued = Assert.IsType<IssuedPatientIntakeAccessLinkDto>(result.Link);
                Assert.Equal(seed.BranchA.Id, issued.BranchId);
                Assert.Equal(InitialUtc.AddMinutes(30), issued.ExpiresAtUtc);
                Assert.NotEmpty(issued.AccessToken);
            }

            await using (var listContext = CreateContext(databaseName, tenantAContext))
            {
                var queryService = new PatientIntakeAccessLinkQueryService(
                    new EfPatientIntakeAccessLinkRepository(listContext),
                    tenantAContext,
                    timeProvider);
                var links = await queryService.ListAsync(
                    includeResolved: false,
                    take: 50);

                var active = Assert.Single(links);
                Assert.Equal(issued.Id, active.Id);
                Assert.Equal("Active", active.Status);
                Assert.DoesNotContain(
                    "Token",
                    active.GetType().GetProperties().Select(property => property.Name));
            }

            await using (var verificationContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var link = await verificationContext.PatientIntakeAccessLinks.SingleAsync();
                var audit = await verificationContext.PatientIntakeAccessLinkAuditEntries.SingleAsync();

                Assert.NotEqual(issued.AccessToken, link.TokenHash);
                Assert.True(tokenService.VerifyHash(issued.AccessToken, link.TokenHash));
                Assert.Equal(PatientIntakeAccessLinkAuditAction.Issued, audit.Action);
                Assert.DoesNotContain(
                    "Token",
                    audit.GetType().GetProperties().Select(property => property.Name));
            }

            timeProvider.Advance(TimeSpan.FromMinutes(1));
            await using (var revokeContext = CreateContext(databaseName, tenantAContext))
            {
                var service = CreateCommandService(
                    revokeContext,
                    tenantAContext,
                    timeProvider,
                    tokenService);
                var result = await service.RevokeAsync(
                    issued.Id,
                    "revoke-waiting-room-1");

                Assert.True(result.Succeeded);
            }

            await using (var finalContext = CreateContext(
                             databaseName,
                             new TenantContext()))
            {
                var link = await finalContext.PatientIntakeAccessLinks.SingleAsync();
                var audits = await finalContext.PatientIntakeAccessLinkAuditEntries
                    .OrderBy(entry => entry.OccurredAtUtc)
                    .ToListAsync();

                Assert.Equal(InitialUtc.AddMinutes(1), link.RevokedAtUtc);
                Assert.Equal(2, audits.Count);
                Assert.Equal(PatientIntakeAccessLinkAuditAction.Issued, audits[0].Action);
                Assert.Equal(PatientIntakeAccessLinkAuditAction.Revoked, audits[1].Action);
            }

            var tenantBContext = CreateStaffContext(seed.UserB.Id, seed.TenantB.Id);
            await using (var foreignListContext = CreateContext(databaseName, tenantBContext))
            {
                Assert.Empty(await foreignListContext.PatientIntakeAccessLinks.ToListAsync());
                Assert.Empty(await foreignListContext.PatientIntakeAccessLinkAuditEntries.ToListAsync());
            }
        }

        [Fact]
        public async Task Issue_RejectsForeignBranchAndPlatformScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedTwoTenantsAsync(databaseName);
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();
            var tenantAContext = CreateStaffContext(seed.UserA.Id, seed.TenantA.Id);

            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                var service = CreateCommandService(
                    context,
                    tenantAContext,
                    timeProvider,
                    tokenService);
                var result = await service.IssueAsync(
                    seed.BranchB.Id,
                    "foreign-branch");

                Assert.False(result.Succeeded);
                Assert.Equal(
                    PatientIntakeAccessLinkIssueFailure.BranchUnavailable,
                    result.Failure);
            }

            var platformContext = new TenantContext();
            platformContext.SetRequestContext(
                Guid.NewGuid().ToString(),
                AccessScope.Platform,
                isAuthenticated: true);
            await using (var platformDbContext = CreateContext(databaseName, platformContext))
            {
                var service = CreateCommandService(
                    platformDbContext,
                    platformContext,
                    timeProvider,
                    tokenService);

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    service.IssueAsync(null, "platform-issue"));
            }

            await using var verificationContext = CreateContext(
                databaseName,
                new TenantContext());
            Assert.Empty(await verificationContext.PatientIntakeAccessLinks.ToListAsync());
            Assert.Empty(await verificationContext.PatientIntakeAccessLinkAuditEntries.ToListAsync());
        }

        [Fact]
        public async Task AuditIsAppendOnlyAndModelContainsTenantConcurrencyGuardrails()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seed = await SeedTwoTenantsAsync(databaseName);
            var timeProvider = new MutableTimeProvider(InitialUtc);
            var tokenService = new PatientIntakeAccessLinkTokenService();
            var tenantAContext = CreateStaffContext(seed.UserA.Id, seed.TenantA.Id);

            await using (var issueContext = CreateContext(databaseName, tenantAContext))
            {
                var result = await CreateCommandService(
                        issueContext,
                        tenantAContext,
                        timeProvider,
                        tokenService)
                    .IssueAsync(seed.BranchA.Id, "append-only-seed");
                Assert.True(result.Succeeded);
            }

            await using (var mutationContext = CreateContext(databaseName, tenantAContext))
            {
                var audit = await mutationContext.PatientIntakeAccessLinkAuditEntries.SingleAsync();
                mutationContext.Entry(audit)
                    .Property(nameof(PatientIntakeAccessLinkAuditEntry.CorrelationId))
                    .CurrentValue = "tampered";

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    mutationContext.SaveChangesAsync());
                Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
            }

            await using var metadataContext = CreateContext(
                Guid.NewGuid().ToString(),
                new TenantContext());
            var linkType = metadataContext.Model.FindEntityType(typeof(PatientIntakeAccessLink))
                ?? throw new InvalidOperationException("PatientIntakeAccessLink metadata was not found.");
            var auditType = metadataContext.Model.FindEntityType(typeof(PatientIntakeAccessLinkAuditEntry))
                ?? throw new InvalidOperationException("PatientIntakeAccessLinkAuditEntry metadata was not found.");

            var tokenIndex = linkType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[] { nameof(PatientIntakeAccessLink.TokenHash) }));
            Assert.True(tokenIndex.IsUnique);
            Assert.True(linkType.FindProperty(nameof(PatientIntakeAccessLink.RowVersion))!.IsConcurrencyToken);
            Assert.NotNull(linkType.GetQueryFilter());
            Assert.NotNull(auditType.GetQueryFilter());

            var constraintNames = linkType.GetCheckConstraints()
                .Select(constraint => constraint.Name)
                .ToHashSet(StringComparer.Ordinal);
            Assert.Contains("CK_PatientIntakeAccessLinks_ExpiryOrder", constraintNames);
            Assert.Contains("CK_PatientIntakeAccessLinks_RevocationMetadata", constraintNames);
            Assert.Contains("CK_PatientIntakeAccessLinks_ConsumptionMetadata", constraintNames);
            Assert.Contains("CK_PatientIntakeAccessLinks_SingleResolution", constraintNames);
        }

        private static PatientIntakeAccessLinkCommandService CreateCommandService(
            AppDbContext context,
            TenantContext tenantContext,
            TimeProvider timeProvider,
            PatientIntakeAccessLinkTokenService tokenService)
        {
            return new PatientIntakeAccessLinkCommandService(
                new EfPatientIntakeAccessLinkRepository(context),
                new EfBranchRepository(context),
                tokenService,
                new FixedSettings(TimeSpan.FromMinutes(30)),
                tenantContext,
                timeProvider);
        }

        private static async Task<SeedData> SeedTwoTenantsAsync(string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            var branchA = tenantA.AddBranch("Main A");
            var branchB = tenantB.AddBranch("Main B");
            var userA = new User("admin-a@example.com", "hashed", "Admin A");
            var userB = new User("admin-b@example.com", "hashed", "Admin B");

            context.Tenants.AddRange(tenantA, tenantB);
            context.Users.AddRange(userA, userB);
            await context.SaveChangesAsync();

            return new SeedData(
                tenantA,
                branchA,
                userA,
                tenantB,
                branchB,
                userB);
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
            Tenant TenantA,
            Branch BranchA,
            User UserA,
            Tenant TenantB,
            Branch BranchB,
            User UserB);

        private sealed class FixedSettings : IPatientIntakeAccessLinkSettings
        {
            public FixedSettings(TimeSpan lifetime)
            {
                WaitingRoomLinkLifetime = lifetime;
            }

            public TimeSpan WaitingRoomLinkLifetime { get; }
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
