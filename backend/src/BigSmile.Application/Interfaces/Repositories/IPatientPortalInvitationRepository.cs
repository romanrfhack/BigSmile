using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientPortalInvitationRepository
    {
        Task<IReadOnlyList<PatientPortalInvitation>> ListByPatientIdAsync(
            Guid patientId,
            int take,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PatientPortalInvitation>> ListOutstandingByPatientIdAsync(
            Guid patientId,
            PatientPortalInvitationPurpose purpose,
            CancellationToken cancellationToken = default);

        Task<PatientPortalInvitation?> GetByIdAsync(
            Guid patientId,
            Guid invitationId,
            CancellationToken cancellationToken = default);

        Task SaveIssueAsync(
            PatientPortalInvitation invitation,
            IReadOnlyCollection<PatientPortalInvitation> supersededInvitations,
            IReadOnlyCollection<PatientPortalSecurityAuditEntry> supersededAuditEntries,
            PatientPortalSecurityAuditEntry issuedAuditEntry,
            CancellationToken cancellationToken = default);

        Task SaveRevocationAsync(
            PatientPortalInvitation invitation,
            PatientPortalSecurityAuditEntry auditEntry,
            CancellationToken cancellationToken = default);
    }
}
