using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfTreatmentQuoteRepository : ITreatmentQuoteRepository
    {
        private readonly AppDbContext _dbContext;

        public EfTreatmentQuoteRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TreatmentQuote?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TreatmentQuotes
                .Include(treatmentQuote => treatmentQuote.Items)
                .SingleOrDefaultAsync(treatmentQuote => treatmentQuote.PatientId == patientId, cancellationToken);
        }

        public async Task<TreatmentQuote?> GetByTreatmentPlanIdAsync(Guid treatmentPlanId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TreatmentQuotes
                .Include(treatmentQuote => treatmentQuote.Items)
                .SingleOrDefaultAsync(treatmentQuote => treatmentQuote.TreatmentPlanId == treatmentPlanId, cancellationToken);
        }

        public async Task AddAsync(TreatmentQuote treatmentQuote, CancellationToken cancellationToken = default)
        {
            _dbContext.TreatmentQuotes.Add(treatmentQuote);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(TreatmentQuote treatmentQuote, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(treatmentQuote).State == EntityState.Detached)
            {
                _dbContext.TreatmentQuotes.Update(treatmentQuote);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
