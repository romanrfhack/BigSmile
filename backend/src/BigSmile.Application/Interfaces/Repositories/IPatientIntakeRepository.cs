using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientIntakeRepository
    {
        Task<PatientIntake?> GetDraftByAccountAsync(
            Guid tenantId,
            Guid patientPortalAccountId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<bool> TryCreateAsync(
            PatientIntake intake,
            PatientIntake? expiredDraft,
            CancellationToken cancellationToken = default);

        Task<bool> TrySaveAsync(
            PatientIntake intake,
            CancellationToken cancellationToken = default);
    }
}
