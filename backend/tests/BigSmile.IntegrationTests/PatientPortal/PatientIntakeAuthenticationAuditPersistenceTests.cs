using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.PatientPortal
{
    public sealed class PatientIntakeAuthenticationAuditPersistenceTests
    {
        private static readonly DateTime OccurredAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task AuditEntries_AreTenantFilteredAndAppendOnly()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seeded = await SeedAuditEntriesAsync(databaseName);

            var tenantAContext = CreateTenantContext(seeded.TenantA.Id);
            await using (var filteredContext = CreateContext(
                             databaseName,
                             tenantAContext))
            {
                var entry = Assert.Single(
                    await filteredContext.PatientIntakeAuthenticationAuditEntries
                        .ToListAsync());
                Assert.Equal(seeded.TenantA.Id, entry.TenantId);

                filteredContext.Entry(entry)
                    .Property(nameof(PatientIntakeAuthenticationAuditEntry.CorrelationId))
                    .CurrentValue = "tampered";
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    filteredContext.SaveChangesAsync());
                Assert.Contains(
                    "append-only",
                    exception.Message,
                    StringComparison.OrdinalIgnoreCase);
            }

            var tenantBContext = CreateTenantContext(seeded.TenantB.Id);
            await using (var filteredContext = CreateContext(
                             databaseName,
                             tenantBContext))
            {
                var entry = Assert.Single(
                    await filteredContext.PatientIntakeAuthenticationAuditEntries
                        .ToListAsync());
                Assert.Equal(seeded.TenantB.Id, entry.TenantId);
            }
        }

        private static async Task<SeededAudit> SeedAuditEntriesAsync(string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            var accountA = PatientPortalAccount.CreateUnlinked(
                tenantA.Id,
                "patient-a",
                "versioned-hash-a",
                OccurredAtUtc);
            var accountB = PatientPortalAccount.CreateUnlinked(
                tenantB.Id,
                "patient-b",
                "versioned-hash-b",
                OccurredAtUtc);
            var intakeA = PatientIntake.CreateForNewPatient(
                accountA,
                branch: null,
                OccurredAtUtc);
            var intakeB = PatientIntake.CreateForNewPatient(
                accountB,
                branch: null,
                OccurredAtUtc);
            var auditA = new PatientIntakeAuthenticationAuditEntry(
                accountA,
                intakeA,
                PatientIntakeAuthenticationAuditAction.AccountActivated,
                PatientIntakeAuthenticationAuditActorType.PatientPortalAccount,
                accountA.Id,
                OccurredAtUtc,
                "tenant-a-audit");
            var auditB = new PatientIntakeAuthenticationAuditEntry(
                accountB,
                intakeB,
                PatientIntakeAuthenticationAuditAction.AccountActivated,
                PatientIntakeAuthenticationAuditActorType.PatientPortalAccount,
                accountB.Id,
                OccurredAtUtc,
                "tenant-b-audit");

            context.Tenants.AddRange(tenantA, tenantB);
            context.PatientPortalAccounts.AddRange(accountA, accountB);
            context.PatientIntakes.AddRange(intakeA, intakeB);
            context.PatientIntakeAuthenticationAuditEntries.AddRange(auditA, auditB);
            await context.SaveChangesAsync();

            return new SeededAudit(tenantA, tenantB);
        }

        private static TenantContext CreateTenantContext(Guid tenantId)
        {
            var context = new TenantContext();
            context.SetRequestContext(
                Guid.NewGuid().ToString(),
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

        private sealed record SeededAudit(Tenant TenantA, Tenant TenantB);
    }
}
