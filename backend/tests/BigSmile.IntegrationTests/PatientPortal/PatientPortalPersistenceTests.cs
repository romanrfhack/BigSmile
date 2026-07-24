using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.PatientPortal
{
    public sealed class PatientPortalPersistenceTests
    {
        [Fact]
        public async Task QueryFilters_KeepAccountsAndInvitationsInsideResolvedTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, patientA, tenantB, patientB) = await SeedTenantPatientsAsync(databaseName);

            await using (var seedContext = CreateContext(databaseName, new TenantContext()))
            {
                var accountA = PatientPortalAccount.CreateForExistingPatient(
                    patientA,
                    "patient-a",
                    "versioned-hash-a");
                var accountB = PatientPortalAccount.CreateForExistingPatient(
                    patientB,
                    "patient-b",
                    "versioned-hash-b");
                var createdAtUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);

                seedContext.PatientPortalAccounts.AddRange(accountA, accountB);
                seedContext.PatientPortalInvitations.AddRange(
                    new PatientPortalInvitation(
                        patientA,
                        PatientPortalInvitationPurpose.ExistingPatientActivation,
                        new string('a', 64),
                        createdAtUtc,
                        createdAtUtc.AddHours(24),
                        Guid.NewGuid()),
                    new PatientPortalInvitation(
                        patientB,
                        PatientPortalInvitationPurpose.ExistingPatientActivation,
                        new string('b', 64),
                        createdAtUtc,
                        createdAtUtc.AddHours(24),
                        Guid.NewGuid()));
                await seedContext.SaveChangesAsync();
            }

            var tenantAContext = CreateTenantContext(tenantA.Id);
            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                var account = Assert.Single(await context.PatientPortalAccounts.ToListAsync());
                var invitation = Assert.Single(await context.PatientPortalInvitations.ToListAsync());

                Assert.Equal(tenantA.Id, account.TenantId);
                Assert.Equal(patientA.Id, account.PatientId);
                Assert.Equal(tenantA.Id, invitation.TenantId);
                Assert.Equal(patientA.Id, invitation.PatientId);
            }

            var tenantBContext = CreateTenantContext(tenantB.Id);
            await using (var context = CreateContext(databaseName, tenantBContext))
            {
                var account = Assert.Single(await context.PatientPortalAccounts.ToListAsync());
                var invitation = Assert.Single(await context.PatientPortalInvitations.ToListAsync());

                Assert.Equal(tenantB.Id, account.TenantId);
                Assert.Equal(patientB.Id, account.PatientId);
                Assert.Equal(tenantB.Id, invitation.TenantId);
                Assert.Equal(patientB.Id, invitation.PatientId);
            }
        }

        [Fact]
        public async Task SaveChanges_BlocksCrossTenantAccountAndInvitationWrites()
        {
            var databaseName = Guid.NewGuid().ToString();
            var (tenantA, _, _, patientB) = await SeedTenantPatientsAsync(databaseName);
            var tenantAContext = CreateTenantContext(tenantA.Id);

            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                context.PatientPortalAccounts.Add(PatientPortalAccount.CreateForExistingPatient(
                    patientB,
                    "foreign-account",
                    "versioned-hash"));

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
                Assert.Contains("target tenant does not match", exception.Message, StringComparison.OrdinalIgnoreCase);
            }

            await using (var context = CreateContext(databaseName, tenantAContext))
            {
                var createdAtUtc = new DateTime(2026, 7, 24, 12, 0, 0, DateTimeKind.Utc);
                context.PatientPortalInvitations.Add(new PatientPortalInvitation(
                    patientB,
                    PatientPortalInvitationPurpose.ExistingPatientActivation,
                    new string('c', 64),
                    createdAtUtc,
                    createdAtUtc.AddHours(24),
                    Guid.NewGuid()));

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync());
                Assert.Contains("target tenant does not match", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void Model_ContainsRequiredUniquenessConcurrencyAndQueryFilterGuardrails()
        {
            using var context = CreateContext(Guid.NewGuid().ToString(), new TenantContext());
            var accountType = context.Model.FindEntityType(typeof(PatientPortalAccount))
                ?? throw new InvalidOperationException("PatientPortalAccount model metadata was not found.");
            var invitationType = context.Model.FindEntityType(typeof(PatientPortalInvitation))
                ?? throw new InvalidOperationException("PatientPortalInvitation model metadata was not found.");

            var loginIndex = accountType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(PatientPortalAccount.TenantId),
                        nameof(PatientPortalAccount.NormalizedLoginName)
                    }));
            var patientIndex = accountType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(PatientPortalAccount.TenantId),
                        nameof(PatientPortalAccount.PatientId)
                    }));
            var tokenIndex = invitationType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[] { nameof(PatientPortalInvitation.TokenHash) }));

            Assert.True(loginIndex.IsUnique);
            Assert.True(patientIndex.IsUnique);
            Assert.Equal("[PatientId] IS NOT NULL", patientIndex.GetFilter());
            Assert.True(tokenIndex.IsUnique);
            Assert.True(accountType.FindProperty(nameof(PatientPortalAccount.RowVersion))!.IsConcurrencyToken);
            Assert.True(invitationType.FindProperty(nameof(PatientPortalInvitation.RowVersion))!.IsConcurrencyToken);
            Assert.NotNull(accountType.GetQueryFilter());
            Assert.NotNull(invitationType.GetQueryFilter());
        }

        private static async Task<(Tenant TenantA, Patient PatientA, Tenant TenantB, Patient PatientB)> SeedTenantPatientsAsync(
            string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            var patientA = CreatePatient(tenantA.Id, "Ana", "Lopez");
            var patientB = CreatePatient(tenantB.Id, "Bruno", "Garcia");

            context.Tenants.AddRange(tenantA, tenantB);
            context.Patients.AddRange(patientA, patientB);
            await context.SaveChangesAsync();

            return (tenantA, patientA, tenantB, patientB);
        }

        private static Patient CreatePatient(Guid tenantId, string firstName, string lastName)
        {
            return new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14));
        }

        private static TenantContext CreateTenantContext(Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(
                Guid.NewGuid().ToString(),
                AccessScope.Tenant,
                isAuthenticated: true,
                tenantId.ToString());
            return tenantContext;
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;
            var configuration = new ConfigurationBuilder().Build();
            return new AppDbContext(options, configuration, tenantContext);
        }
    }
}
