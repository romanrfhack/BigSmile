using BigSmile.Application.Features.PatientPortalAuthentication.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientPortalAuthentication.Commands
{
    public interface IPatientPortalPublicAuthenticationService
    {
        Task<PatientPortalActivationResult> ActivateAsync(
            ActivatePatientPortalAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task<PatientPortalAuthenticationResponseDto?> LoginAsync(
            LoginPatientPortalAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientPortalPublicAuthenticationService : IPatientPortalPublicAuthenticationService
    {
        private const int TenantSubdomainMaxLength = 100;
        private readonly IPatientPortalAuthenticationRepository _repository;
        private readonly IPatientPortalInvitationTokenService _invitationTokenService;
        private readonly IPatientPortalPasswordHasher _passwordHasher;
        private readonly IPatientPortalAuthenticationSettings _settings;
        private readonly IPatientPortalJwtTokenService _jwtTokenService;
        private readonly TimeProvider _timeProvider;

        public PatientPortalPublicAuthenticationService(
            IPatientPortalAuthenticationRepository repository,
            IPatientPortalInvitationTokenService invitationTokenService,
            IPatientPortalPasswordHasher passwordHasher,
            IPatientPortalAuthenticationSettings settings,
            IPatientPortalJwtTokenService jwtTokenService,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _invitationTokenService = invitationTokenService ?? throw new ArgumentNullException(nameof(invitationTokenService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<PatientPortalActivationResult> ActivateAsync(
            ActivatePatientPortalAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var normalizedLoginName = PatientPortalAccount.NormalizeLoginNameForLookup(command.LoginName);
            ValidatePassword(command.Password);

            if (string.IsNullOrWhiteSpace(command.ActivationToken) || command.ActivationToken.Length > 256)
            {
                return PatientPortalActivationResult.Failed(PatientPortalActivationFailure.InvalidActivation);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var tokenHash = _invitationTokenService.ComputeHash(command.ActivationToken);
            var invitation = await _repository.GetInvitationByTokenHashAsync(tokenHash, cancellationToken);

            if (invitation is null ||
                !_invitationTokenService.VerifyHash(command.ActivationToken, invitation.TokenHash) ||
                !invitation.CanBeConsumedAt(now) ||
                !invitation.Patient.IsActive ||
                !invitation.Tenant.IsActive ||
                string.IsNullOrWhiteSpace(invitation.Tenant.Subdomain))
            {
                return PatientPortalActivationResult.Failed(PatientPortalActivationFailure.InvalidActivation);
            }

            var existingAccount = await _repository.GetAccountByPatientAsync(
                invitation.TenantId,
                invitation.PatientId,
                trackChanges: true,
                cancellationToken);

            if (existingAccount is not null && existingAccount.IsActive)
            {
                return PatientPortalActivationResult.Failed(PatientPortalActivationFailure.InvalidActivation);
            }

            if (await _repository.LoginNameExistsAsync(
                    invitation.TenantId,
                    normalizedLoginName,
                    existingAccount?.Id,
                    cancellationToken))
            {
                return PatientPortalActivationResult.Failed(PatientPortalActivationFailure.LoginNameUnavailable);
            }

            var passwordHash = _passwordHasher.HashPassword(command.Password);
            var isNewAccount = existingAccount is null;
            var account = existingAccount ?? PatientPortalAccount.CreateForExistingPatient(
                invitation.Patient,
                command.LoginName,
                passwordHash,
                now);

            var activationAction = PatientPortalAuthenticationAuditAction.AccountActivated;
            if (!isNewAccount)
            {
                account.CompleteRecovery(command.LoginName, passwordHash, now);
                activationAction = PatientPortalAuthenticationAuditAction.AccountRecovered;
            }

            invitation.Consume(account, now);

            var audits = new[]
            {
                new PatientPortalAuthenticationAuditEntry(
                    account,
                    activationAction,
                    PatientPortalAuthenticationAuditActorType.PatientPortalAccount,
                    account.Id,
                    now,
                    correlationId,
                    invitation),
                new PatientPortalAuthenticationAuditEntry(
                    account,
                    PatientPortalAuthenticationAuditAction.InvitationConsumed,
                    PatientPortalAuthenticationAuditActorType.PatientPortalAccount,
                    account.Id,
                    now,
                    correlationId,
                    invitation)
            };

            try
            {
                await _repository.SaveActivationAsync(
                    account,
                    isNewAccount,
                    invitation,
                    audits,
                    cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return PatientPortalActivationResult.Failed(PatientPortalActivationFailure.ConcurrentConflict);
            }

            return PatientPortalActivationResult.Success(BuildAuthenticationResponse(
                account,
                invitation.Tenant.Subdomain!,
                now));
        }

        public async Task<PatientPortalAuthenticationResponseDto?> LoginAsync(
            LoginPatientPortalAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            var providedPassword = command.Password ?? string.Empty;

            string normalizedSubdomain;
            string normalizedLoginName;
            try
            {
                normalizedSubdomain = NormalizeTenantSubdomain(command.TenantSubdomain);
                normalizedLoginName = PatientPortalAccount.NormalizeLoginNameForLookup(command.LoginName);
            }
            catch (ArgumentException)
            {
                _passwordHasher.PerformDummyVerification(providedPassword);
                return null;
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var tenant = await _repository.GetActiveTenantBySubdomainAsync(normalizedSubdomain, cancellationToken);
            if (tenant is null)
            {
                _passwordHasher.PerformDummyVerification(providedPassword);
                return null;
            }

            var account = await _repository.GetAccountByLoginAsync(
                tenant.Id,
                normalizedLoginName,
                trackChanges: true,
                cancellationToken);

            if (account is null ||
                !account.PatientId.HasValue ||
                account.PatientId.Value == Guid.Empty ||
                account.Patient is null ||
                !account.Patient.IsActive)
            {
                _passwordHasher.PerformDummyVerification(providedPassword);
                return null;
            }

            if (!account.IsActive || account.IsLockedOutAt(now))
            {
                _passwordHasher.PerformDummyVerification(providedPassword);
                return null;
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                account.PasswordHash,
                providedPassword);

            if (verification == PatientPortalPasswordVerificationStatus.Failed)
            {
                var locked = account.RegisterFailedLogin(
                    now,
                    _settings.MaximumFailedLoginAttempts,
                    _settings.LockoutDuration);

                var audits = new List<PatientPortalAuthenticationAuditEntry>
                {
                    new(
                        account,
                        PatientPortalAuthenticationAuditAction.LoginFailed,
                        PatientPortalAuthenticationAuditActorType.PatientPortalAccount,
                        account.Id,
                        now,
                        correlationId)
                };

                if (locked)
                {
                    audits.Add(new PatientPortalAuthenticationAuditEntry(
                        account,
                        PatientPortalAuthenticationAuditAction.AccountLocked,
                        PatientPortalAuthenticationAuditActorType.PatientPortalAccount,
                        account.Id,
                        now,
                        correlationId));
                }

                try
                {
                    await _repository.SaveAccountStateAsync(account, audits, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    // Authentication failures remain generic even when the account changed concurrently.
                }

                return null;
            }

            if (verification == PatientPortalPasswordVerificationStatus.SuccessRehashNeeded)
            {
                account.UpdatePasswordHash(_passwordHasher.HashPassword(providedPassword), now);
            }

            account.RegisterSuccessfulLogin(now);
            var loginAudit = new PatientPortalAuthenticationAuditEntry(
                account,
                PatientPortalAuthenticationAuditAction.LoginSucceeded,
                PatientPortalAuthenticationAuditActorType.PatientPortalAccount,
                account.Id,
                now,
                correlationId);

            try
            {
                await _repository.SaveAccountStateAsync(account, new[] { loginAudit }, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            return BuildAuthenticationResponse(account, tenant.Subdomain!, now);
        }

        private PatientPortalAuthenticationResponseDto BuildAuthenticationResponse(
            PatientPortalAccount account,
            string tenantSubdomain,
            DateTime issuedAtUtc)
        {
            if (!account.PatientId.HasValue)
            {
                throw new InvalidOperationException("Patient portal authentication requires an account linked to a patient.");
            }

            var generatedToken = _jwtTokenService.Generate(account, issuedAtUtc);
            return new PatientPortalAuthenticationResponseDto(
                generatedToken.Token,
                generatedToken.ExpiresAtUtc,
                new CurrentPatientPortalSessionDto(
                    account.Id,
                    account.PatientId.Value,
                    tenantSubdomain,
                    account.LoginName,
                    account.SessionVersion));
        }

        private void ValidatePassword(string? password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Patient portal password is required.", nameof(password));
            }

            if (password.Length < _settings.MinimumPasswordLength ||
                password.Length > _settings.MaximumPasswordLength)
            {
                throw new ArgumentException(
                    $"Patient portal password must contain between {_settings.MinimumPasswordLength} and {_settings.MaximumPasswordLength} characters.",
                    nameof(password));
            }

            if (password.Any(char.IsControl))
            {
                throw new ArgumentException("Patient portal password cannot contain control characters.", nameof(password));
            }
        }

        internal static string NormalizeTenantSubdomain(string? tenantSubdomain)
        {
            if (string.IsNullOrWhiteSpace(tenantSubdomain))
            {
                throw new ArgumentException("Tenant realm is required.", nameof(tenantSubdomain));
            }

            var normalized = tenantSubdomain.Trim().ToLowerInvariant();
            if (normalized.Length > TenantSubdomainMaxLength ||
                normalized.StartsWith('-') ||
                normalized.EndsWith('-') ||
                normalized.Any(character => !char.IsLetterOrDigit(character) && character != '-'))
            {
                throw new ArgumentException("Tenant realm has an unsupported format.", nameof(tenantSubdomain));
            }

            return normalized;
        }
    }
}
