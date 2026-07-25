using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfPatientIntakeRepository : IPatientIntakeRepository
    {
        private readonly AppDbContext _dbContext;

        public EfPatientIntakeRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<PatientIntake?> GetDraftByAccountAsync(
            Guid tenantId,
            Guid patientPortalAccountId,
            bool trackChanges,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.PatientIntakes
                .Include(intake => intake.PatientPortalAccount)
                .Include(intake => intake.MedicalAnswers)
                .Where(intake => intake.TenantId == tenantId &&
                                 intake.PatientPortalAccountId == patientPortalAccountId &&
                                 intake.Status == PatientIntakeStatus.Draft);

            return ApplyTracking(query, trackChanges)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> TryCreateAsync(
            PatientIntake intake,
            PatientIntake? expiredDraft,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(intake);

            IDbContextTransaction? transaction = null;
            try
            {
                if (_dbContext.Database.IsRelational())
                {
                    transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                }

                if (expiredDraft is not null && _dbContext.Database.IsRelational())
                {
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                _dbContext.PatientIntakes.Add(intake);
                await _dbContext.SaveChangesAsync(cancellationToken);

                if (transaction is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return true;
            }
            catch (DbUpdateException)
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

        public async Task<bool> TrySaveAsync(
            PatientIntake intake,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(intake);

            foreach (var revision in intake.Revisions)
            {
                if (_dbContext.Entry(revision).State == EntityState.Detached)
                {
                    _dbContext.PatientIntakeRevisions.Add(revision);
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }

        private static IQueryable<PatientIntake> ApplyTracking(
            IQueryable<PatientIntake> query,
            bool trackChanges)
        {
            return trackChanges ? query : query.AsNoTracking();
        }
    }
}
