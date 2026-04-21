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
        public DbSet<Odontogram> Odontograms => Set<Odontogram>();
        public DbSet<OdontogramToothState> OdontogramToothStates => Set<OdontogramToothState>();
        public DbSet<OdontogramSurfaceState> OdontogramSurfaceStates => Set<OdontogramSurfaceState>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<AppointmentBlock> AppointmentBlocks => Set<AppointmentBlock>();

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

            // Apply configurations from the same assembly
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

            modelBuilder.Entity<Odontogram>().HasQueryFilter(odontogram =>
                !ShouldApplyTenantFilter || odontogram.TenantId == ResolvedTenantId);

            modelBuilder.Entity<Appointment>().HasQueryFilter(appointment =>
                !ShouldApplyTenantFilter || appointment.TenantId == ResolvedTenantId);

            modelBuilder.Entity<AppointmentBlock>().HasQueryFilter(appointmentBlock =>
                !ShouldApplyTenantFilter || appointmentBlock.TenantId == ResolvedTenantId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback configuration (only for design-time migrations)
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=BigSmile;Trusted_Connection=True;MultipleActiveResultSets=true";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        public override int SaveChanges()
        {
            ValidateTenantBoundWrites();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ValidateTenantBoundWrites();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ValidateTenantBoundWrites();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
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
