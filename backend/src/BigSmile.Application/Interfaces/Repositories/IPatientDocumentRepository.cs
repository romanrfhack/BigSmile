using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientDocumentRepository
    {
        Task<IReadOnlyList<PatientDocument>> ListActiveByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<PatientDocument?> GetActiveByIdAsync(Guid patientId, Guid documentId, CancellationToken cancellationToken = default);
        Task AddAsync(PatientDocument patientDocument, CancellationToken cancellationToken = default);
        Task UpdateAsync(PatientDocument patientDocument, CancellationToken cancellationToken = default);
    }
}
