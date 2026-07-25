using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientIntakeAccessLinkRepository
    {
        Task<IReadOnlyList<PatientIntakeAccessLink>> ListAsync(
            DateTime utcNow,
            bool includeResolved,
            int take,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeAccessLink?> GetByIdAsync(
            Guid linkId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<bool> TryIssueAsync(
            PatientIntakeAccessLink link,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default);

        Task<bool> TryRevokeAsync(
            PatientIntakeAccessLink link,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default);
    }
}
