using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientIntakeAuthenticationRepository
        : IPatientIntakeAuthenticationRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientIntakeAuthenticationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<Tenant?> GetActiveTenantBySubdomainAsync(
            string normalizedSubdomain,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Tenants
                .IgnoreQueryFilters()
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    tenant => tenant.IsActive &&
                              tenant.Subdomain != null &&
                              tenant.Subdomain.ToLower() == normalizedSubdomain,
                    cancellationToken);
        }

        public Task<PatientIntakeAccessLink?> GetAccessLinkByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.PatientIntakeAccessLinks
                .IgnoreQueryFilters()
                .Include(link => link.Tenant)
                .Include(link => link.Branch)
                .SingleOrDefaultAsync(
                    link => link.TokenHash == tokenHash,
                    cancellationToken);
        }

        public Task<PatientPortalAccount?> GetUnlinkedAccountByLoginAsync(
            Guid tenantId,
            string normalizedLoginName,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .Include(account => account.Tenant)
                .Where(account => account.TenantId == tenantId &&
                                  account.PatientId == null &&
                                  account.NormalizedLoginName == normalizedLoginName);

            return ApplyTracking(query, trackChanges)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<PatientPortalAccount?> GetAccountForSessionAsync(
            Guid accountId,
            Guid tenantId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .Include(account => account.Tenant)
                .Where(account => account.Id == accountId &&
                                  account.TenantId == tenantId &&
                                  account.PatientId == null);

            return ApplyTracking(query, trackChanges)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<PatientIntake?> GetCurrentDraftByAccountAsync(
            Guid tenantId,
            Guid accountId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientIntakes
                .IgnoreQueryFilters()
                .Include(intake => intake.MedicalAnswers)
                .Include(intake => intake.Revisions)
                .Where(intake => intake.TenantId == tenantId &&
                                 intake.PatientPortalAccountId == accountId &&
                                 intake.Origin == PatientIntakeOrigin.NewPatientWaitingRoom &&
                                 intake.Status == PatientIntakeStatus.Draft);

            return ApplyTracking(query, trackChanges)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<PatientIntake?> GetIntakeForSessionAsync(
            Guid intakeId,
            Guid accountId,
            Guid tenantId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientIntakes
                .IgnoreQueryFilters()
                .Include(intake => intake.MedicalAnswers)
                .Include(intake => intake.Revisions)
                .Where(intake => intake.Id == intakeId &&
                                 intake.PatientPortalAccountId == accountId &&
                                 intake.TenantId == tenantId &&
                                 intake.Origin == PatientIntakeOrigin.NewPatientWaitingRoom);

            return ApplyTracking(query, trackChanges)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public Task<bool> LoginNameExistsAsync(
            Guid tenantId,
            string normalizedLoginName,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .AnyAsync(
                    account => account.TenantId == tenantId &&
                               account.NormalizedLoginName == normalizedLoginName,
                    cancellationToken);
        }

        public async Task<bool> TryActivateAsync(
            PatientPortalAccount account,
            PatientIntake intake,
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditEntry accessLinkAuditEntry,
            IReadOnlyCollection<PatientIntakeAuthenticationAuditEntry> authenticationAuditEntries,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(intake);
            ArgumentNullException.ThrowIfNull(accessLink);
            ArgumentNullException.ThrowIfNull(accessLinkAuditEntry);
            ArgumentNullException.ThrowIfNull(authenticationAuditEntries);

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                }

                _dbContext.PatientPortalAccounts.Add(account);
                _dbContext.PatientIntakes.Add(intake);
                if (_dbContext.Entry(accessLink).State == EntityState.Detached)
                {
                    _dbContext.PatientIntakeAccessLinks.Update(accessLink);
                }

                _dbContext.PatientIntakeAccessLinkAuditEntries.Add(accessLinkAuditEntry);
                _dbContext.PatientIntakeAuthenticationAuditEntries.AddRange(authenticationAuditEntries);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return true;
            }
            catch (Exception exception) when (
                exception is DbUpdateException or InvalidOperationException)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                return false;
            }
            finally
            {
                if (transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        public async Task<bool> TrySaveAccountStateAsync(
            PatientPortalAccount account,
            IReadOnlyCollection<PatientIntakeAuthenticationAuditEntry> auditEntries,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(auditEntries);

            if (_dbContext.Entry(account).State == EntityState.Detached)
            {
                _dbContext.PatientPortalAccounts.Update(account);
            }

            _dbContext.PatientIntakeAuthenticationAuditEntries.AddRange(auditEntries);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (Exception exception) when (
                exception is DbUpdateException or InvalidOperationException)
            {
                return false;
            }
        }

        private static IQueryable<T> ApplyTracking<T>(
            IQueryable<T> query,
            bool trackChanges)
            where T : class
        {
            return trackChanges ? query : query.AsNoTracking();
        }
    }
}
