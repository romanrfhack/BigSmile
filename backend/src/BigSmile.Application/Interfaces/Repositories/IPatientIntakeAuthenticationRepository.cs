using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Repositories
{
    public interface IPatientIntakeAuthenticationRepository
    {
        Task<Tenant?> GetActiveTenantBySubdomainAsync(
            string normalizedSubdomain,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeAccessLink?> GetAccessLinkByTokenHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default);

        Task<PatientPortalAccount?> GetUnlinkedAccountByLoginAsync(
            Guid tenantId,
            string normalizedLoginName,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<PatientPortalAccount?> GetAccountForSessionAsync(
            Guid accountId,
            Guid tenantId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<PatientIntake?> GetCurrentDraftByAccountAsync(
            Guid tenantId,
            Guid accountId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<PatientIntake?> GetIntakeForSessionAsync(
            Guid intakeId,
            Guid accountId,
            Guid tenantId,
            bool trackChanges,
            CancellationToken cancellationToken = default);

        Task<bool> LoginNameExistsAsync(
            Guid tenantId,
            string normalizedLoginName,
            CancellationToken cancellationToken = default);

        Task<bool> TryActivateAsync(
            PatientPortalAccount account,
            PatientIntake intake,
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditEntry accessLinkAuditEntry,
            IReadOnlyCollection<PatientIntakeAuthenticationAuditEntry> authenticationAuditEntries,
            CancellationToken cancellationToken = default);

        Task<bool> TrySaveAccountStateAsync(
            PatientPortalAccount account,
            IReadOnlyCollection<PatientIntakeAuthenticationAuditEntry> auditEntries,
            CancellationToken cancellationToken = default);
    }
}
