using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.PatientPortal
{
    public sealed class PatientIntakePersistenceTests
    {
        private static readonly DateTime CreatedAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task QueryFilters_KeepIntakesAnswersAndRevisionsInsideResolvedTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seeded = await SeedTwoTenantIntakesAsync(databaseName);

            await using (var context = CreateContext(
                             databaseName,
                             CreateTenantContext(seeded.TenantA.Id)))
            {
                var intake = Assert.Single(await context.PatientIntakes.ToListAsync());
                var answers = await context.PatientIntakeMedicalAnswers.ToListAsync();
                var revision = Assert.Single(await context.PatientIntakeRevisions.ToListAsync());

                Assert.Equal(seeded.TenantA.Id, intake.TenantId);
                Assert.Equal(seeded.PatientA.Id, intake.PatientId);
                Assert.Equal(39, answers.Count);
                Assert.All(answers, answer => Assert.Equal(seeded.TenantA.Id, answer.TenantId));
                Assert.Equal(seeded.TenantA.Id, revision.TenantId);
                Assert.Equal(intake.Id, revision.PatientIntakeId);
            }

            await using (var context = CreateContext(
                             databaseName,
                             CreateTenantContext(seeded.TenantB.Id)))
            {
                var intake = Assert.Single(await context.PatientIntakes.ToListAsync());
                var answers = await context.PatientIntakeMedicalAnswers.ToListAsync();
                var revision = Assert.Single(await context.PatientIntakeRevisions.ToListAsync());

                Assert.Equal(seeded.TenantB.Id, intake.TenantId);
                Assert.Equal(seeded.PatientB.Id, intake.PatientId);
                Assert.Equal(39, answers.Count);
                Assert.All(answers, answer => Assert.Equal(seeded.TenantB.Id, answer.TenantId));
                Assert.Equal(seeded.TenantB.Id, revision.TenantId);
                Assert.Equal(intake.Id, revision.PatientIntakeId);
            }
        }

        [Fact]
        public async Task SaveChanges_BlocksCrossTenantIntakeWrites()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seeded = await SeedTwoTenantIntakesAsync(databaseName);
            var tenantAContext = CreateTenantContext(seeded.TenantA.Id);
            var foreignDraft = PatientIntake.CreateForExistingPatient(
                seeded.AccountB,
                seeded.PatientB,
                seeded.BranchB,
                CreatedAtUtc.AddDays(1));

            await using var context = CreateContext(databaseName, tenantAContext);
            context.AttachRange(seeded.AccountB, seeded.PatientB, seeded.BranchB);
            context.PatientIntakes.Add(foreignDraft);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => context.SaveChangesAsync());
            Assert.Contains(
                "target tenant does not match",
                exception.Message,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SaveChanges_BlocksRevisionModificationAndDeletion()
        {
            var databaseName = Guid.NewGuid().ToString();
            var seeded = await SeedTwoTenantIntakesAsync(databaseName);
            var tenantContext = CreateTenantContext(seeded.TenantA.Id);

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var revision = Assert.Single(await context.PatientIntakeRevisions.ToListAsync());
                context.Entry(revision)
                    .Property(nameof(PatientIntakeRevision.SnapshotJson))
                    .CurrentValue = "{\"tampered\":true}";

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => context.SaveChangesAsync());
                Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
            }

            await using (var context = CreateContext(databaseName, tenantContext))
            {
                var revision = Assert.Single(await context.PatientIntakeRevisions.ToListAsync());
                context.PatientIntakeRevisions.Remove(revision);

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                    () => context.SaveChangesAsync());
                Assert.Contains("append-only", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void Model_ContainsIntakeUniquenessConcurrencyFiltersAndCheckConstraints()
        {
            using var context = CreateContext(Guid.NewGuid().ToString(), new TenantContext());
            var model = context.GetService<IDesignTimeModel>().Model;
            var intakeType = model.FindEntityType(typeof(PatientIntake))
                ?? throw new InvalidOperationException("PatientIntake model metadata was not found.");
            var answerType = model.FindEntityType(typeof(PatientIntakeMedicalAnswer))
                ?? throw new InvalidOperationException("PatientIntakeMedicalAnswer model metadata was not found.");
            var revisionType = model.FindEntityType(typeof(PatientIntakeRevision))
                ?? throw new InvalidOperationException("PatientIntakeRevision model metadata was not found.");

            var activeDraftIndex = intakeType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(PatientIntake.TenantId),
                        nameof(PatientIntake.PatientPortalAccountId)
                    }));
            var answerIndex = answerType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(PatientIntakeMedicalAnswer.TenantId),
                        nameof(PatientIntakeMedicalAnswer.PatientIntakeId),
                        nameof(PatientIntakeMedicalAnswer.QuestionKey)
                    }));
            var revisionIndex = revisionType.GetIndexes().Single(index =>
                index.Properties.Select(property => property.Name)
                    .SequenceEqual(new[]
                    {
                        nameof(PatientIntakeRevision.TenantId),
                        nameof(PatientIntakeRevision.PatientIntakeId),
                        nameof(PatientIntakeRevision.RevisionNumber)
                    }));

            Assert.True(activeDraftIndex.IsUnique);
            Assert.Equal("[Status] = N'Draft'", activeDraftIndex.GetFilter());
            Assert.True(answerIndex.IsUnique);
            Assert.True(revisionIndex.IsUnique);
            Assert.True(intakeType.FindProperty(nameof(PatientIntake.RowVersion))!.IsConcurrencyToken);
            Assert.NotNull(intakeType.GetQueryFilter());
            Assert.NotNull(answerType.GetQueryFilter());
            Assert.NotNull(revisionType.GetQueryFilter());

            var intakeConstraints = intakeType.GetCheckConstraints()
                .Select(constraint => constraint.Name)
                .ToHashSet(StringComparer.Ordinal);
            Assert.Contains("CK_PatientIntakes_OriginPatientLink", intakeConstraints);
            Assert.Contains("CK_PatientIntakes_ExpiryOrder", intakeConstraints);
            Assert.Contains("CK_PatientIntakes_CurrentRevisionNumber", intakeConstraints);
        }

        private static async Task<SeededIntakes> SeedTwoTenantIntakesAsync(string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            var branchA = tenantA.AddBranch("Main A");
            var branchB = tenantB.AddBranch("Main B");
            var patientA = CreatePatient(tenantA.Id, "Ana", "Lopez");
            var patientB = CreatePatient(tenantB.Id, "Bruno", "Garcia");
            var accountA = PatientPortalAccount.CreateForExistingPatient(
                patientA,
                "patient-a",
                "versioned-hash-a");
            var accountB = PatientPortalAccount.CreateForExistingPatient(
                patientB,
                "patient-b",
                "versioned-hash-b");
            var intakeA = PatientIntake.CreateForExistingPatient(
                accountA,
                patientA,
                branchA,
                CreatedAtUtc);
            var intakeB = PatientIntake.CreateForExistingPatient(
                accountB,
                patientB,
                branchB,
                CreatedAtUtc);

            intakeA.SaveDraft(
                BuildDraft("Ana updated"),
                accountA.Id,
                CreatedAtUtc.AddMinutes(1),
                "tenant-a-save");
            intakeB.SaveDraft(
                BuildDraft("Bruno updated"),
                accountB.Id,
                CreatedAtUtc.AddMinutes(1),
                "tenant-b-save");

            context.Tenants.AddRange(tenantA, tenantB);
            context.Patients.AddRange(patientA, patientB);
            context.PatientPortalAccounts.AddRange(accountA, accountB);
            context.PatientIntakes.AddRange(intakeA, intakeB);
            await context.SaveChangesAsync();

            return new SeededIntakes(
                tenantA,
                patientA,
                branchA,
                accountA,
                tenantB,
                patientB,
                branchB,
                accountB);
        }

        private static PatientIntakeDraftData BuildDraft(string firstName)
        {
            return PatientIntakeDraftData.Empty() with
            {
                FirstName = firstName
            };
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

        private sealed record SeededIntakes(
            Tenant TenantA,
            Patient PatientA,
            Branch BranchA,
            PatientPortalAccount AccountA,
            Tenant TenantB,
            Patient PatientB,
            Branch BranchB,
            PatientPortalAccount AccountB);
    }
}
