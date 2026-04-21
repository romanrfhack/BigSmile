using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface ITreatmentQuoteRepository
    {
        Task<TreatmentQuote?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<TreatmentQuote?> GetByTreatmentPlanIdAsync(Guid treatmentPlanId, CancellationToken cancellationToken = default);
        Task AddAsync(TreatmentQuote treatmentQuote, CancellationToken cancellationToken = default);
        Task UpdateAsync(TreatmentQuote treatmentQuote, CancellationToken cancellationToken = default);
    }
}
