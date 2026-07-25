using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
            int take,
            CancellationToken cancellationToken = default)
        {
            if (take <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take));
            }

            return await _dbContext.PatientIntakeAccessLinks
                .AsNoTracking()
                .Include(accessLink => accessLink.Branch)
                .OrderByDescending(accessLink => accessLink.CreatedAtUtc)
                .ThenByDescending(accessLink => accessLink.Id)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public Task<PatientIntakeAccessLink?> GetByIdAsync(
            Guid accessLinkId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientIntakeAccessLinks
                .Include(accessLink => accessLink.Branch)
                .Where(accessLink => accessLink.Id == accessLinkId);

            return (trackChanges ? query : query.AsNoTracking())
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task SaveIssueAsync(
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(accessLink);
            ArgumentNullException.ThrowIfNull(auditEntry);

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                }

                _dbContext.PatientIntakeAccessLinks.Add(accessLink);
                _dbContext.PatientIntakeAccessLinkAuditEntries.Add(auditEntry);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (Exception exception) when (
                exception is DbUpdateException or InvalidOperationException)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw new InvalidOperationException(
                    "Patient intake access link could not be issued because its state changed or the generated credential conflicted.",
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

        public async Task SaveRevocationAsync(
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(accessLink);
            ArgumentNullException.ThrowIfNull(auditEntry);

            if (_dbContext.Entry(accessLink).State == EntityState.Detached)
            {
                _dbContext.PatientIntakeAccessLinks.Update(accessLink);
            }

            _dbContext.PatientIntakeAccessLinkAuditEntries.Add(auditEntry);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception exception) when (
                exception is DbUpdateException or InvalidOperationException)
            {
                throw new InvalidOperationException(
                    "Patient intake access link could not be revoked because its state changed concurrently.",
                    exception);
            }
        }
    }
}
