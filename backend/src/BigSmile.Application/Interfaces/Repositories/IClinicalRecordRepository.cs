using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IClinicalRecordRepository
    {
        Task<ClinicalRecord?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task AddAsync(ClinicalRecord clinicalRecord, CancellationToken cancellationToken = default);
        Task UpdateAsync(ClinicalRecord clinicalRecord, CancellationToken cancellationToken = default);
    }
}
