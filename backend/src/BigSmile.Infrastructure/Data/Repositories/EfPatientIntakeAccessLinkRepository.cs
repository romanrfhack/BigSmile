using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientIntakeAccessLinkRepository
        : IPatientIntakeAccessLinkRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientIntakeAccessLinkRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<PatientIntakeAccessLink>> ListAsync(
            DateTime utcNow,
            bool includeResolved,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientIntakeAccessLinks
                .AsNoTracking();

            if (!includeResolved)
            {
                query = query.Where(link =>
                    link.RevokedAtUtc == null &&
                    link.ConsumedAtUtc == null &&
                    link.ExpiresAtUtc > utcNow);
            }

            return await query
                .OrderByDescending(link => link.CreatedAtUtc)
                .ThenByDescending(link => link.Id)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public Task<PatientIntakeAccessLink?> GetByIdAsync(
            Guid linkId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientIntakeAccessLinks
                .Where(link => link.Id == linkId);

            return (trackChanges ? query : query.AsNoTracking())
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> TryIssueAsync(
            PatientIntakeAccessLink link,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(link);
            ArgumentNullException.ThrowIfNull(auditEntry);

            _dbContext.PatientIntakeAccessLinks.Add(link);
            _dbContext.PatientIntakeAccessLinkAuditEntries.Add(auditEntry);

            return await TrySaveChangesAsync(cancellationToken);
        }

        public async Task<bool> TryRevokeAsync(
            PatientIntakeAccessLink link,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(link);
            ArgumentNullException.ThrowIfNull(auditEntry);

            if (_dbContext.Entry(link).State == EntityState.Detached)
            {
                _dbContext.PatientIntakeAccessLinks.Update(link);
            }

            _dbContext.PatientIntakeAccessLinkAuditEntries.Add(auditEntry);
            return await TrySaveChangesAsync(cancellationToken);
        }

        private async Task<bool> TrySaveChangesAsync(
            CancellationToken cancellationToken)
        {
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
    }
}
