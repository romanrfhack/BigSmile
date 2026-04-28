using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IReminderTemplateRepository
    {
        Task<ReminderTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReminderTemplate>> ListAsync(
            bool includeInactive = false,
            CancellationToken cancellationToken = default);
        Task AddAsync(ReminderTemplate template, CancellationToken cancellationToken = default);
        Task UpdateAsync(ReminderTemplate template, CancellationToken cancellationToken = default);
    }
}
