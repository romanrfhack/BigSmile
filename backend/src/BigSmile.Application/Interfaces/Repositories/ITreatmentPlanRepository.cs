using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface ITreatmentPlanRepository
    {
        Task<TreatmentPlan?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task AddAsync(TreatmentPlan treatmentPlan, CancellationToken cancellationToken = default);
        Task UpdateAsync(TreatmentPlan treatmentPlan, CancellationToken cancellationToken = default);
    }
}
