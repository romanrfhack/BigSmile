using BigSmile.Application.Features.PatientPortalAuthentication.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientPortalAuthentication.Commands
{
    public interface IPatientPortalSessionService
    {
        Task<CurrentPatientPortalSessionDto?> GetCurrentAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default);

        Task<bool> RevokeCurrentSessionsAsync(
            PatientPortalSessionIdentity identity,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientPortalSessionService : IPatientPortalSessionService
    {
        private readonly IPatientPortalAuthenticationRepository _repository;
        private readonly TimeProvider _timeProvider;

        public PatientPortalSessionService(
            IPatientPortalAuthenticationRepository repository,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<CurrentPatientPortalSessionDto?> GetCurrentAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            var account = await _repository.GetAccountForSessionAsync(
                identity.AccountId,
                identity.TenantId,
                identity.PatientId,
                trackChanges: false,
                cancellationToken);

            if (!IsCurrentSession(account, identity) ||
                account!.Tenant is null ||
                !account.Tenant.IsActive ||
                string.IsNullOrWhiteSpace(account.Tenant.Subdomain))
            {
                return null;
            }

            return new CurrentPatientPortalSessionDto(
                account.Id,
                account.PatientId!.Value,
                account.Tenant.Subdomain!,
                account.LoginName,
                account.SessionVersion);
        }

        public async Task<bool> RevokeCurrentSessionsAsync(
            PatientPortalSessionIdentity identity,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var account = await _repository.GetAccountForSessionAsync(
                identity.AccountId,
                identity.TenantId,
                identity.PatientId,
                trackChanges: true,
                cancellationToken);

            if (!IsCurrentSession(account, identity))
            {
                return false;
            }

            var occurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            account!.RevokeSessions(occurredAtUtc);
            var auditEntry = new PatientPortalAuthenticationAuditEntry(
                account,
                PatientPortalAuthenticationAuditAction.SessionsRevoked,
                PatientPortalAuthenticationAuditActorType.PatientPortalAccount,
                account.Id,
                occurredAtUtc,
                correlationId);

            try
            {
                await _repository.SaveAccountStateAsync(
                    account,
                    new[] { auditEntry },
                    cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            return true;
        }

        private static bool IsCurrentSession(
            PatientPortalAccount? account,
            PatientPortalSessionIdentity identity)
        {
            return account is not null &&
                   account.IsActive &&
                   account.Patient is not null &&
                   account.Patient.IsActive &&
                   account.PatientId == identity.PatientId &&
                   account.TenantId == identity.TenantId &&
                   account.SessionVersion == identity.SessionVersion;
        }
    }
}
