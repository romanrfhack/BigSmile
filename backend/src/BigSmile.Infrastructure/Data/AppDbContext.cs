using BigSmile.Infrastructure.Context;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Multitenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace BigSmile.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserTenantMembership> UserTenantMemberships => Set<UserTenantMembership>();
        public DbSet<UserBranchAssignment> UserBranchAssignments => Set<UserBranchAssignment>();
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<ClinicalRecord> ClinicalRecords => Set<ClinicalRecord>();
        public DbSet<ClinicalDiagnosis> ClinicalDiagnoses => Set<ClinicalDiagnosis>();
        public DbSet<ClinicalSnapshotHistoryEntry> ClinicalSnapshotHistoryEntries => Set<ClinicalSnapshotHistoryEntry>();
        public DbSet<ClinicalMedicalAnswer> ClinicalMedicalAnswers => Set<ClinicalMedicalAnswer>();
        public DbSet<ClinicalEncounter> ClinicalEncounters => Set<ClinicalEncounter>();
        public DbSet<Odontogram> Odontograms => Set<Odontogram>();
        public DbSet<OdontogramToothState> OdontogramToothStates => Set<OdontogramToothState>();
        public DbSet<OdontogramSurfaceState> OdontogramSurfaceStates => Set<OdontogramSurfaceState>();
        public DbSet<OdontogramSurfaceFinding> OdontogramSurfaceFindings => Set<OdontogramSurfaceFinding>();
        public DbSet<OdontogramSurfaceFindingHistoryEntry> OdontogramSurfaceFindingHistoryEntries => Set<OdontogramSurfaceFindingHistoryEntry>();
        public DbSet<PatientDocument> PatientDocuments => Set<PatientDocument>();
        public DbSet<PatientPortalAccount> PatientPortalAccounts => Set<PatientPortalAccount>();
        public DbSet<PatientPortalInvitation> PatientPortalInvitations => Set<PatientPortalInvitation>();
        public DbSet<PatientPortalSecurityAuditEntry> PatientPortalSecurityAuditEntries => Set<PatientPortalSecurityAuditEntry>();
        public DbSet<PatientPortalAuthenticationAuditEntry> PatientPortalAuthenticationAuditEntries => Set<PatientPortalAuthenticationAuditEntry>();
        public DbSet<PatientIntake> PatientIntakes => Set<PatientIntake>();
        public DbSet<PatientIntakeMedicalAnswer> PatientIntakeMedicalAnswers => Set<PatientIntakeMedicalAnswer>();
        public DbSet<PatientIntakeRevision> PatientIntakeRevisions => Set<PatientIntakeRevision>();
        public DbSet<PatientIntakeAccessLink> PatientIntakeAccessLinks => Set<PatientIntakeAccessLink>();
        public DbSet<PatientIntakeAccessLinkAuditEntry> PatientIntakeAccessLinkAuditEntries => Set<PatientIntakeAccessLinkAuditEntry>();
        public DbSet<PatientIntakeAuthenticationAuditEntry> PatientIntakeAuthenticationAuditEntries => Set<PatientIntakeAuthenticationAuditEntry>();
        public DbSet<BillingDocument> BillingDocuments => Set<BillingDocument>();
        public DbSet<BillingDocumentItem> BillingDocumentItems => Set<BillingDocumentItem>();
        public DbSet<TreatmentPlan> TreatmentPlans => Set<TreatmentPlan>();
        public DbSet<TreatmentPlanItem> TreatmentPlanItems => Set<TreatmentPlanItem>();
        public DbSet<TreatmentQuote> TreatmentQuotes => Set<TreatmentQuote>();
        public DbSet<TreatmentQuoteItem> TreatmentQuoteItems => Set<TreatmentQuoteItem>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppointmentBlock> AppointmentBlocks => Set<AppointmentBlock>();
        public DbSet<AppointmentReminderLogEntry> AppointmentReminderLogEntries => Set<AppointmentReminderLogEntry>();
        public DbSet<ReminderTemplate> ReminderTemplates => Set<ReminderTemplate>();

        private readonly IConfiguration _configuration;
        private readonly TenantContext _tenantContext;

        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration, TenantContext tenantContext)
            : base(options)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<Tenant>().HasQueryFilter(tenant =>
                !ShouldApplyTenantFilter || tenant.Id == ResolvedTenantId);
            modelBuilder.Entity<Branch>().HasQueryFilter(branch =>
                !ShouldApplyTenantFilter || branch.TenantId == ResolvedTenantId);
            modelBuilder.Entity<UserTenantMembership>().HasQueryFilter(membership =>
                !ShouldApplyTenantFilter || membership.TenantId == ResolvedTenantId);
            modelBuilder.Entity<Patient>().HasQueryFilter(patient =>
                !ShouldApplyTenantFilter || patient.TenantId == ResolvedTenantId);
            modelBuilder.Entity<ClinicalRecord>().HasQueryFilter(clinicalRecord =>
                !ShouldApplyTenantFilter || clinicalRecord.TenantId == ResolvedTenantId);
            modelBuilder.Entity<ClinicalMedicalAnswer>().HasQueryFilter(answer =>
                !ShouldApplyTenantFilter || answer.TenantId == ResolvedTenantId);
            modelBuilder.Entity<ClinicalEncounter>().HasQueryFilter(encounter =>
                !ShouldApplyTenantFilter || encounter.TenantId == ResolvedTenantId);
            modelBuilder.Entity<Odontogram>().HasQueryFilter(odontogram =>
                !ShouldApplyTenantFilter || odontogram.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientDocument>().HasQueryFilter(patientDocument =>
                !ShouldApplyTenantFilter || patientDocument.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientPortalAccount>().HasQueryFilter(account =>
                !ShouldApplyTenantFilter || account.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientPortalInvitation>().HasQueryFilter(invitation =>
                !ShouldApplyTenantFilter || invitation.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientPortalSecurityAuditEntry>().HasQueryFilter(entry =>
                !ShouldApplyTenantFilter || entry.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientPortalAuthenticationAuditEntry>().HasQueryFilter(entry =>
                !ShouldApplyTenantFilter || entry.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientIntake>().HasQueryFilter(intake =>
                !ShouldApplyTenantFilter || intake.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientIntakeMedicalAnswer>().HasQueryFilter(answer =>
                !ShouldApplyTenantFilter || answer.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientIntakeRevision>().HasQueryFilter(revision =>
                !ShouldApplyTenantFilter || revision.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientIntakeAccessLink>().HasQueryFilter(link =>
                !ShouldApplyTenantFilter || link.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientIntakeAccessLinkAuditEntry>().HasQueryFilter(entry =>
                !ShouldApplyTenantFilter || entry.TenantId == ResolvedTenantId);
            modelBuilder.Entity<PatientIntakeAuthenticationAuditEntry>().HasQueryFilter(entry =>
                !ShouldApplyTenantFilter || entry.TenantId == ResolvedTenantId);
            modelBuilder.Entity<BillingDocument>().HasQueryFilter(billingDocument =>
                !ShouldApplyTenantFilter || billingDocument.TenantId == ResolvedTenantId);
            modelBuilder.Entity<TreatmentPlan>().HasQueryFilter(treatmentPlan =>
                !ShouldApplyTenantFilter || treatmentPlan.TenantId == ResolvedTenantId);
            modelBuilder.Entity<TreatmentQuote>().HasQueryFilter(treatmentQuote =>
                !ShouldApplyTenantFilter || treatmentQuote.TenantId == ResolvedTenantId);
            modelBuilder.Entity<Appointment>().HasQueryFilter(appointment =>
                !ShouldApplyTenantFilter || appointment.TenantId == ResolvedTenantId);
            modelBuilder.Entity<AppointmentBlock>().HasQueryFilter(appointmentBlock =>
                !ShouldApplyTenantFilter || appointmentBlock.TenantId == ResolvedTenantId);
            modelBuilder.Entity<AppointmentReminderLogEntry>().HasQueryFilter(entry =>
                !ShouldApplyTenantFilter || entry.TenantId == ResolvedTenantId);
            modelBuilder.Entity<ReminderTemplate>().HasQueryFilter(template =>
                !ShouldApplyTenantFilter || template.TenantId == ResolvedTenantId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=BigSmile;Trusted_Connection=True;MultipleActiveResultSets=true";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public override int SaveChanges()
        {
            ValidateAppendOnlyEntries();
            ValidateTenantBoundWrites();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ValidateAppendOnlyEntries();
            ValidateTenantBoundWrites();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ValidateAppendOnlyEntries();
            ValidateTenantBoundWrites();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ValidateAppendOnlyEntries();
            ValidateTenantBoundWrites();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private Guid? CurrentTenantId =>
            Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId)
                ? tenantId
                : null;

        private Guid ResolvedTenantId => CurrentTenantId ?? Guid.Empty;

        private bool ShouldApplyTenantFilter =>
            _tenantContext.IsAuthenticated() && !_tenantContext.HasPlatformOverride();

        private void ValidateAppendOnlyEntries()
        {
            var invalidSecurityAuditEntry = ChangeTracker.Entries<PatientPortalSecurityAuditEntry>()
                .FirstOrDefault(entry => entry.State is EntityState.Modified or EntityState.Deleted);
            if (invalidSecurityAuditEntry is not null)
            {
                throw new InvalidOperationException(
                    "Patient portal security audit entries are append-only and cannot be modified or deleted.");
            }

            var invalidAuthenticationAuditEntry = ChangeTracker.Entries<PatientPortalAuthenticationAuditEntry>()
                .FirstOrDefault(entry => entry.State is EntityState.Modified or EntityState.Deleted);
            if (invalidAuthenticationAuditEntry is not null)
            {
                throw new InvalidOperationException(
                    "Patient portal authentication audit entries are append-only and cannot be modified or deleted.");
            }

            var invalidPatientIntakeRevision = ChangeTracker.Entries<PatientIntakeRevision>()
                .FirstOrDefault(entry => entry.State is EntityState.Modified or EntityState.Deleted);
            if (invalidPatientIntakeRevision is not null)
            {
                throw new InvalidOperationException(
                    "Patient intake revisions are append-only and cannot be modified or deleted.");
            }

            var invalidAccessLinkAuditEntry = ChangeTracker.Entries<PatientIntakeAccessLinkAuditEntry>()
                .FirstOrDefault(entry => entry.State is EntityState.Modified or EntityState.Deleted);
            if (invalidAccessLinkAuditEntry is not null)
            {
                throw new InvalidOperationException(
                    "Patient intake access link audit entries are append-only and cannot be modified or deleted.");
            }

            var invalidIntakeAuthenticationAuditEntry = ChangeTracker.Entries<PatientIntakeAuthenticationAuditEntry>()
                .FirstOrDefault(entry => entry.State is EntityState.Modified or EntityState.Deleted);
            if (invalidIntakeAuthenticationAuditEntry is not null)
            {
                throw new InvalidOperationException(
                    "Patient intake authentication audit entries are append-only and cannot be modified or deleted.");
            }
        }

        private void ValidateTenantBoundWrites()
        {
            if (!_tenantContext.IsAuthenticated() || _tenantContext.HasPlatformOverride())
            {
                return;
            }

            if (!CurrentTenantId.HasValue)
            {
                throw new InvalidOperationException("Authenticated tenant-scoped writes require a resolved tenant context.");
            }

            foreach (var entry in ChangeTracker.Entries()
                         .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                switch (entry.Entity)
                {
                    case Tenant tenant when tenant.Id != CurrentTenantId.Value:
                        throw new InvalidOperationException(
                            $"Tenant write '{entry.State}' was blocked because the target tenant does not match the current tenant context.");
                    case ITenantOwnedEntity tenantOwnedEntity when tenantOwnedEntity.TenantId != CurrentTenantId.Value:
                        throw new InvalidOperationException(
                            $"Tenant-owned write '{entry.State}' was blocked because the target tenant does not match the current tenant context.");
                }
            }
        }
    }
}
