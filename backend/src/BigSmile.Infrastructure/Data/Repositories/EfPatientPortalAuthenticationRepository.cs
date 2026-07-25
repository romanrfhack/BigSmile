using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientPortalAuthenticationRepository : IPatientPortalAuthenticationRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientPortalAuthenticationRepository(AppDbContext dbContext)
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

        public Task<PatientPortalInvitation?> GetInvitationByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.PatientPortalInvitations
                .IgnoreQueryFilters()
                .Include(invitation => invitation.Patient)
                .Include(invitation => invitation.Tenant)
                .SingleOrDefaultAsync(invitation => invitation.TokenHash == tokenHash, cancellationToken);
        }

        public Task<PatientPortalAccount?> GetAccountByLoginAsync(
            Guid tenantId,
            string normalizedLoginName,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .Include(account => account.Tenant)
                .Include(account => account.Patient)
                .Where(account => account.TenantId == tenantId &&
                                  account.NormalizedLoginName == normalizedLoginName);

            return ApplyTracking(query, trackChanges).SingleOrDefaultAsync(cancellationToken);
        }

        public Task<PatientPortalAccount?> GetAccountByPatientAsync(
            Guid tenantId,
            Guid patientId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .Include(account => account.Tenant)
                .Include(account => account.Patient)
                .Where(account => account.TenantId == tenantId && account.PatientId == patientId);

            return ApplyTracking(query, trackChanges).SingleOrDefaultAsync(cancellationToken);
        }

        public Task<PatientPortalAccount?> GetAccountForSessionAsync(
            Guid accountId,
            Guid tenantId,
            Guid patientId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .Include(account => account.Tenant)
                .Include(account => account.Patient)
                .Where(account => account.Id == accountId &&
                                  account.TenantId == tenantId &&
                                  account.PatientId == patientId);

            return ApplyTracking(query, trackChanges).SingleOrDefaultAsync(cancellationToken);
        }

        public Task<bool> LoginNameExistsAsync(
            Guid tenantId,
            string normalizedLoginName,
            Guid? excludedAccountId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.PatientPortalAccounts
                .IgnoreQueryFilters()
                .AnyAsync(
                    account => account.TenantId == tenantId &&
                               account.NormalizedLoginName == normalizedLoginName &&
                               (!excludedAccountId.HasValue || account.Id != excludedAccountId.Value),
                    cancellationToken);
        }

        public async Task SaveActivationAsync(
            PatientPortalAccount account,
            bool isNewAccount,
            PatientPortalInvitation invitation,
            IReadOnlyCollection<PatientPortalAuthenticationAuditEntry> auditEntries,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(invitation);
            ArgumentNullException.ThrowIfNull(auditEntries);

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                }

                if (isNewAccount)
                {
                    _dbContext.PatientPortalAccounts.Add(account);
                }
                else if (_dbContext.Entry(account).State == EntityState.Detached)
                {
                    _dbContext.PatientPortalAccounts.Update(account);
                }

                if (_dbContext.Entry(invitation).State == EntityState.Detached)
                {
                    _dbContext.PatientPortalInvitations.Update(invitation);
                }

                _dbContext.PatientPortalAuthenticationAuditEntries.AddRange(auditEntries);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw new InvalidOperationException(
                    "Patient portal activation could not be completed because the credential changed concurrently.",
                    exception);
            }
            finally
            {
                if (transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        public async Task SaveAccountStateAsync(
            PatientPortalAccount account,
            IReadOnlyCollection<PatientPortalAuthenticationAuditEntry> auditEntries,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(auditEntries);

            if (_dbContext.Entry(account).State == EntityState.Detached)
            {
                _dbContext.PatientPortalAccounts.Update(account);
            }

            _dbContext.PatientPortalAuthenticationAuditEntries.AddRange(auditEntries);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException)
            {
                throw new InvalidOperationException(
                    "Patient portal account state changed concurrently.",
                    exception);
            }
        }

        public async Task SaveRecoveryAsync(
            PatientPortalAccount account,
            PatientPortalInvitation invitation,
            IReadOnlyCollection<PatientPortalInvitation> supersededInvitations,
            IReadOnlyCollection<PatientPortalSecurityAuditEntry> invitationAuditEntries,
            PatientPortalAuthenticationAuditEntry recoveryAuditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(invitation);
            ArgumentNullException.ThrowIfNull(supersededInvitations);
            ArgumentNullException.ThrowIfNull(invitationAuditEntries);
            ArgumentNullException.ThrowIfNull(recoveryAuditEntry);

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                }

                if (_dbContext.Entry(account).State == EntityState.Detached)
                {
                    _dbContext.PatientPortalAccounts.Update(account);
                }

                foreach (var supersededInvitation in supersededInvitations)
                {
                    if (_dbContext.Entry(supersededInvitation).State == EntityState.Detached)
                    {
                        _dbContext.PatientPortalInvitations.Update(supersededInvitation);
                    }
                }

                _dbContext.PatientPortalInvitations.Add(invitation);
                _dbContext.PatientPortalSecurityAuditEntries.AddRange(invitationAuditEntries);
                _dbContext.PatientPortalAuthenticationAuditEntries.Add(recoveryAuditEntry);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (Exception exception) when (exception is DbUpdateException or InvalidOperationException)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw new InvalidOperationException(
                    "Patient portal recovery could not be completed because account or invitation state changed concurrently.",
                    exception);
            }
            finally
            {
                if (transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        private static IQueryable<PatientPortalAccount> ApplyTracking(
            IQueryable<PatientPortalAccount> query,
            bool trackChanges)
        {
            return trackChanges ? query : query.AsNoTracking();
        }
    }
}
