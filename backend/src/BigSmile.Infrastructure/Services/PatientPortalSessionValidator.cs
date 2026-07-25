using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalSessionValidator : IPatientPortalSessionValidator
    {
        private readonly IPatientPortalAuthenticationRepository _repository;

        public PatientPortalSessionValidator(IPatientPortalAuthenticationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> ValidateAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            if (identity.AccountId == Guid.Empty ||
                identity.TenantId == Guid.Empty ||
                identity.PatientId == Guid.Empty ||
                identity.SessionVersion <= 0)
            {
                return false;
            }

            var account = await _repository.GetAccountForSessionAsync(
                identity.AccountId,
                identity.TenantId,
                identity.PatientId,
                trackChanges: false,
                cancellationToken);

            return account is not null &&
                   account.IsActive &&
                   account.PatientId == identity.PatientId &&
                   account.SessionVersion == identity.SessionVersion &&
                   account.Tenant.IsActive;
        }
    }
}
