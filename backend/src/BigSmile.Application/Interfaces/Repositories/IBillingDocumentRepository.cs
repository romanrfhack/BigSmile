using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IBillingDocumentRepository
    {
        Task<BillingDocument?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<BillingDocument?> GetByTreatmentQuoteIdAsync(Guid treatmentQuoteId, CancellationToken cancellationToken = default);
        Task AddAsync(BillingDocument billingDocument, CancellationToken cancellationToken = default);
        Task UpdateAsync(BillingDocument billingDocument, CancellationToken cancellationToken = default);
    }
}
