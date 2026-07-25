using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientPortalAuthenticationRepository
    {
        Task<Tenant?> GetActiveTenantBySubdomainAsync(
            string normalizedSubdomain,
            CancellationToken cancellationToken = default);

        Task<PatientPortalInvitation?> GetInvitationByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default);

        Task<PatientPortalAccount?> GetAccountByLoginAsync(
            Guid tenantId,
            string normalizedLoginName,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<PatientPortalAccount?> GetAccountByPatientAsync(
            Guid tenantId,
            Guid patientId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<PatientPortalAccount?> GetAccountForSessionAsync(
            Guid accountId,
            Guid tenantId,
            Guid patientId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<bool> LoginNameExistsAsync(
            Guid tenantId,
            string normalizedLoginName,
            Guid? excludedAccountId,
            CancellationToken cancellationToken = default);

        Task SaveActivationAsync(
            PatientPortalAccount account,
            bool isNewAccount,
            PatientPortalInvitation invitation,
            IReadOnlyCollection<PatientPortalAuthenticationAuditEntry> auditEntries,
            CancellationToken cancellationToken = default);

        Task SaveAccountStateAsync(
            PatientPortalAccount account,
            IReadOnlyCollection<PatientPortalAuthenticationAuditEntry> auditEntries,
            CancellationToken cancellationToken = default);

        Task SaveRecoveryAsync(
            PatientPortalAccount account,
            PatientPortalInvitation invitation,
            IReadOnlyCollection<PatientPortalInvitation> supersededInvitations,
            IReadOnlyCollection<PatientPortalSecurityAuditEntry> invitationAuditEntries,
            PatientPortalAuthenticationAuditEntry recoveryAuditEntry,
            CancellationToken cancellationToken = default);
    }
}
