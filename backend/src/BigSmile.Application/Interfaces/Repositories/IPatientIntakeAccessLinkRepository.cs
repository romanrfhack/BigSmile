using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientIntakeAccessLinkRepository
    {
        Task<IReadOnlyList<PatientIntakeAccessLink>> ListAsync(
            int take,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeAccessLink?> GetByIdAsync(
            Guid accessLinkId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task SaveIssueAsync(
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default);

        Task SaveRevocationAsync(
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditEntry auditEntry,
            CancellationToken cancellationToken = default);
    }
}
