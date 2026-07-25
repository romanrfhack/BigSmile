using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientPortalInvitationRepository : IPatientPortalInvitationRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientPortalInvitationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IReadOnlyList<PatientPortalInvitation>> ListByPatientIdAsync(
            Guid patientId,
            int take,
            CancellationToken cancellationToken = default)
        {
            if (take <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take));
            }

            return await _dbContext.PatientPortalInvitations
                .AsNoTracking()
                .Where(invitation => invitation.PatientId == patientId)
                .OrderByDescending(invitation => invitation.CreatedAtUtc)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<PatientPortalInvitation>> ListOutstandingByPatientIdAsync(
            Guid patientId,
            PatientPortalInvitationPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.PatientPortalInvitations
                .Where(invitation => invitation.PatientId == patientId &&
                                     invitation.Purpose == purpose &&
                                     invitation.RevokedAtUtc == null &&
                                     invitation.ConsumedAtUtc == null)
                .OrderBy(invitation => invitation.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<PatientPortalInvitation?> GetByIdAsync(
            Guid patientId,
            Guid invitationId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.PatientPortalInvitations
                .SingleOrDefaultAsync(
                    invitation => invitation.PatientId == patientId && invitation.Id == invitationId,
                    cancellationToken);
        }

        public async Task SaveIssueAsync(
            PatientPortalInvitation invitation,
            IReadOnlyCollection<PatientPortalInvitation> supersededInvitations,
            IReadOnlyCollection<PatientPortalSecurityAuditEntry> supersededAuditEntries,
            PatientPortalSecurityAuditEntry issuedAuditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(invitation);
            ArgumentNullException.ThrowIfNull(supersededInvitations);
            ArgumentNullException.ThrowIfNull(supersededAuditEntries);
            ArgumentNullException.ThrowIfNull(issuedAuditEntry);

            if (supersededInvitations.Count != supersededAuditEntries.Count)
            {
                throw new ArgumentException(
                    "Every superseded patient portal invitation requires one audit entry.",
                    nameof(supersededAuditEntries));
            }

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                }

                if (supersededInvitations.Count > 0)
                {
                    foreach (var supersededInvitation in supersededInvitations)
                    {
                        if (_dbContext.Entry(supersededInvitation).State == EntityState.Detached)
                        {
                            _dbContext.PatientPortalInvitations.Update(supersededInvitation);
                        }
                    }

                    _dbContext.PatientPortalSecurityAuditEntries.AddRange(supersededAuditEntries);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                _dbContext.PatientPortalInvitations.Add(invitation);
                _dbContext.PatientPortalSecurityAuditEntries.Add(issuedAuditEntry);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }
            }
            catch (DbUpdateException exception)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                }

                throw new InvalidOperationException(
                    "A concurrent patient portal invitation issuance conflict was detected. Retry the operation.",
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
            PatientPortalInvitation invitation,
            PatientPortalSecurityAuditEntry auditEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(invitation);
            ArgumentNullException.ThrowIfNull(auditEntry);

            if (_dbContext.Entry(invitation).State == EntityState.Detached)
            {
                _dbContext.PatientPortalInvitations.Update(invitation);
            }

            _dbContext.PatientPortalSecurityAuditEntries.Add(auditEntry);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                throw new InvalidOperationException(
                    "The patient portal invitation changed concurrently. Refresh and retry the operation.",
                    exception);
            }
        }
    }
}
