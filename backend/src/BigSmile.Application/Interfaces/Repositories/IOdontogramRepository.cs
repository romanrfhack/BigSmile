using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IOdontogramRepository
    {
        Task<Odontogram?> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task AddAsync(Odontogram odontogram, CancellationToken cancellationToken = default);
        Task UpdateAsync(Odontogram odontogram, CancellationToken cancellationToken = default);
    }
}
