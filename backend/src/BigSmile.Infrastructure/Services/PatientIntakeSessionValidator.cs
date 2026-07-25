using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientIntakeSessionValidator
        : IPatientIntakeSessionValidator
    {
        private readonly IPatientIntakeAuthenticationRepository _repository;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeSessionValidator(
            IPatientIntakeAuthenticationRepository repository,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<bool> ValidateAsync(
            PatientIntakeSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(identity);

            var account = await _repository.GetAccountForSessionAsync(
                identity.AccountId,
                identity.TenantId,
                trackChanges: false,
                cancellationToken);
            if (account is null ||
                !account.IsActive ||
                account.PatientId.HasValue ||
                account.Tenant is null ||
                !account.Tenant.IsActive ||
                account.SessionVersion != identity.SessionVersion)
            {
                return false;
            }

            var intake = await _repository.GetIntakeForSessionAsync(
                identity.IntakeId,
                identity.AccountId,
                identity.TenantId,
                trackChanges: false,
                cancellationToken);
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

            return intake is not null &&
                   intake.PatientId is null &&
                   intake.Origin == PatientIntakeOrigin.NewPatientWaitingRoom &&
                   intake.Status == PatientIntakeStatus.Draft &&
                   !intake.IsExpiredAt(utcNow);
        }
    }
}
