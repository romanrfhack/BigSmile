using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfBillingDocumentRepository : IBillingDocumentRepository
    {
        private readonly AppDbContext _dbContext;

        public EfBillingDocumentRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<BillingDocument?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.BillingDocuments
                .Include(billingDocument => billingDocument.Items)
                .SingleOrDefaultAsync(billingDocument => billingDocument.PatientId == patientId, cancellationToken);
        }

        public async Task<BillingDocument?> GetByTreatmentQuoteIdAsync(Guid treatmentQuoteId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.BillingDocuments
                .Include(billingDocument => billingDocument.Items)
                .SingleOrDefaultAsync(billingDocument => billingDocument.TreatmentQuoteId == treatmentQuoteId, cancellationToken);
        }

        public async Task AddAsync(BillingDocument billingDocument, CancellationToken cancellationToken = default)
        {
            _dbContext.BillingDocuments.Add(billingDocument);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(BillingDocument billingDocument, CancellationToken cancellationToken = default)
        {
            if (_dbContext.Entry(billingDocument).State == EntityState.Detached)
            {
                _dbContext.BillingDocuments.Update(billingDocument);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
