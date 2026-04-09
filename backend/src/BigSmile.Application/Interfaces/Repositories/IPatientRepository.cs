using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientRepository
    {
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Patient>> SearchAsync(
            string? searchTerm,
            bool includeInactive,
            int take,
            CancellationToken cancellationToken = default);
        Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
        Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default);
    }
}
