using BigSmile.Application.Features.PatientIntakeAuthentication.Dtos;
using BigSmile.Application.Interfaces.PatientIntakes;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientIntakeAuthentication.Commands
{
    public interface IPatientIntakePublicAuthenticationService
    {
        Task<PatientIntakeActivationResult> ActivateAsync(
            ActivatePatientIntakeAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeAuthenticationResponseDto?> LoginAsync(
            LoginPatientIntakeAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public interface IPatientIntakeSessionService
    {
        Task<CurrentPatientIntakeSessionDto?> GetCurrentAsync(
            PatientIntakeSessionIdentity identity,
            CancellationToken cancellationToken = default);

        Task<bool> RevokeCurrentSessionsAsync(
            PatientIntakeSessionIdentity identity,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientIntakePublicAuthenticationService
        : IPatientIntakePublicAuthenticationService
    {
        private readonly IPatientIntakeAuthenticationRepository _repository;
        private readonly IPatientIntakeAccessLinkTokenService _accessLinkTokenService;
        private readonly IPatientPortalPasswordHasher _passwordHasher;
        private readonly IPatientPortalAuthenticationSettings _authenticationSettings;
        private readonly IPatientPortalJwtTokenService _jwtTokenService;
        private readonly IPatientIntakeDraftSettings _draftSettings;
        private readonly TimeProvider _timeProvider;

        public PatientIntakePublicAuthenticationService(
            IPatientIntakeAuthenticationRepository repository,
            IPatientIntakeAccessLinkTokenService accessLinkTokenService,
            IPatientPortalPasswordHasher passwordHasher,
            IPatientPortalAuthenticationSettings authenticationSettings,
            IPatientPortalJwtTokenService jwtTokenService,
            IPatientIntakeDraftSettings draftSettings,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _accessLinkTokenService = accessLinkTokenService ?? throw new ArgumentNullException(nameof(accessLinkTokenService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _authenticationSettings = authenticationSettings ?? throw new ArgumentNullException(nameof(authenticationSettings));
            _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _draftSettings = draftSettings ?? throw new ArgumentNullException(nameof(draftSettings));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<PatientIntakeActivationResult> ActivateAsync(
            ActivatePatientIntakeAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);
            PatientPortalPassword.Validate(command.Password, _authenticationSettings);

            if (string.IsNullOrWhiteSpace(command.AccessToken) ||
                command.AccessToken.Trim().Length > 256)
            {
                return PatientIntakeActivationResult.Failed(
                    PatientIntakeActivationFailure.InvalidActivation);
            }

            var normalizedLoginName = PatientPortalAccount.NormalizeLoginNameForLookup(
                command.LoginName);
            var tokenHash = _accessLinkTokenService.ComputeHash(command.AccessToken);
            var link = await _repository.GetAccessLinkByTokenHashAsync(
                tokenHash,
                cancellationToken);
            var occurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime;

            if (!IsActiveWaitingRoomLink(link, occurredAtUtc) ||
                !_accessLinkTokenService.VerifyHash(
                    command.AccessToken,
                    link!.TokenHash))
            {
                return PatientIntakeActivationResult.Failed(
                    PatientIntakeActivationFailure.InvalidActivation);
            }

            if (await _repository.LoginNameExistsAsync(
                    link.TenantId,
                    normalizedLoginName,
                    cancellationToken))
            {
                return PatientIntakeActivationResult.Failed(
                    PatientIntakeActivationFailure.LoginNameUnavailable);
            }

            var passwordHash = _passwordHasher.HashPassword(command.Password);
            var account = PatientPortalAccount.CreateUnlinked(
                link.TenantId,
                command.LoginName,
                passwordHash,
                occurredAtUtc);
            var intake = PatientIntake.CreateForNewPatient(
                account,
                link.Branch,
                occurredAtUtc,
                _draftSettings.DraftLifetime);
            link.Consume(account, occurredAtUtc);

            var linkAudit = new PatientIntakeAccessLinkAuditEntry(
                link,
                PatientIntakeAccessLinkAuditAction.Consumed,
                PatientIntakeAccessLinkAuditActorType.PatientPortalAccount,
                account.Id,
                occurredAtUtc,
                correlationId);
            var authenticationAudits = new[]
            {
                new PatientIntakeAuthenticationAuditEntry(
                    account,
                    intake,
                    PatientIntakeAuthenticationAuditAction.AccountActivated,
                    PatientIntakeAuthenticationAuditActorType.PatientPortalAccount,
                    account.Id,
                    occurredAtUtc,
                    correlationId,
                    link),
                new PatientIntakeAuthenticationAuditEntry(
                    account,
                    intake,
                    PatientIntakeAuthenticationAuditAction.LinkConsumed,
                    PatientIntakeAuthenticationAuditActorType.PatientPortalAccount,
                    account.Id,
                    occurredAtUtc,
                    correlationId,
                    link)
            };

            var saved = await _repository.TryActivateAsync(
                account,
                intake,
                link,
                linkAudit,
                authenticationAudits,
                cancellationToken);
            if (!saved)
            {
                return PatientIntakeActivationResult.Failed(
                    PatientIntakeActivationFailure.ConcurrentConflict);
            }

            return PatientIntakeActivationResult.Success(
                BuildAuthenticationResponse(account, intake, occurredAtUtc));
        }

        public async Task<PatientIntakeAuthenticationResponseDto?> LoginAsync(
            LoginPatientIntakeAccountCommand command,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            string normalizedSubdomain;
            string normalizedLoginName;
            try
            {
                normalizedSubdomain = PatientPortalPublicAuthenticationService
                    .NormalizeTenantSubdomain(command.TenantSubdomain);
                normalizedLoginName = PatientPortalAccount.NormalizeLoginNameForLookup(
                    command.LoginName);
            }
            catch (ArgumentException)
            {
                _passwordHasher.PerformDummyVerification(command.Password);
                return null;
            }

            var tenant = await _repository.GetActiveTenantBySubdomainAsync(
                normalizedSubdomain,
                cancellationToken);
            if (tenant is null)
            {
                _passwordHasher.PerformDummyVerification(command.Password);
                return null;
            }

            var account = await _repository.GetUnlinkedAccountByLoginAsync(
                tenant.Id,
                normalizedLoginName,
                trackChanges: true,
                cancellationToken);
            if (account is null || !account.IsActive)
            {
                _passwordHasher.PerformDummyVerification(command.Password);
                return null;
            }

            var intake = await _repository.GetCurrentDraftByAccountAsync(
                tenant.Id,
                account.Id,
                trackChanges: false,
                cancellationToken);
            var occurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            if (intake is null || intake.IsExpiredAt(occurredAtUtc))
            {
                _passwordHasher.PerformDummyVerification(command.Password);
                return null;
            }

            if (account.IsLockedOutAt(occurredAtUtc))
            {
                _passwordHasher.PerformDummyVerification(command.Password);
                return null;
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                account.PasswordHash,
                command.Password);
            if (verification == PatientPortalPasswordVerificationStatus.Failed)
            {
                var locked = account.RegisterFailedLogin(
                    occurredAtUtc,
                    _authenticationSettings.MaximumFailedLoginAttempts,
                    _authenticationSettings.LockoutDuration);
                var audits = new List<PatientIntakeAuthenticationAuditEntry>
                {
                    CreateAccountAudit(
                        account,
                        intake,
                        PatientIntakeAuthenticationAuditAction.LoginFailed,
                        occurredAtUtc,
                        correlationId)
                };
                if (locked)
                {
                    audits.Add(CreateAccountAudit(
                        account,
                        intake,
                        PatientIntakeAuthenticationAuditAction.AccountLocked,
                        occurredAtUtc,
                        correlationId));
                }

                _ = await _repository.TrySaveAccountStateAsync(
                    account,
                    audits,
                    cancellationToken);
                return null;
            }

            account.RegisterSuccessfulLogin(occurredAtUtc);
            if (verification == PatientPortalPasswordVerificationStatus.SuccessRehashNeeded)
            {
                account.UpdatePasswordHash(
                    _passwordHasher.HashPassword(command.Password),
                    occurredAtUtc);
            }

            var loginAudit = CreateAccountAudit(
                account,
                intake,
                PatientIntakeAuthenticationAuditAction.LoginSucceeded,
                occurredAtUtc,
                correlationId);
            var saved = await _repository.TrySaveAccountStateAsync(
                account,
                new[] { loginAudit },
                cancellationToken);

            return saved
                ? BuildAuthenticationResponse(account, intake, occurredAtUtc)
                : null;
        }

        private PatientIntakeAuthenticationResponseDto BuildAuthenticationResponse(
            PatientPortalAccount account,
            PatientIntake intake,
            DateTime issuedAtUtc)
        {
            var token = _jwtTokenService.GenerateForIntake(
                account,
                intake,
                issuedAtUtc);
            return new PatientIntakeAuthenticationResponseDto(
                token.Token,
                token.ExpiresAtUtc,
                new CurrentPatientIntakeSessionDto(
                    account.Id,
                    intake.Id,
                    account.Tenant.Subdomain!,
                    account.LoginName,
                    account.SessionVersion));
        }

        private static PatientIntakeAuthenticationAuditEntry CreateAccountAudit(
            PatientPortalAccount account,
            PatientIntake intake,
            PatientIntakeAuthenticationAuditAction action,
            DateTime occurredAtUtc,
            string correlationId)
        {
            return new PatientIntakeAuthenticationAuditEntry(
                account,
                intake,
                action,
                PatientIntakeAuthenticationAuditActorType.PatientPortalAccount,
                account.Id,
                occurredAtUtc,
                correlationId);
        }

        private static bool IsActiveWaitingRoomLink(
            PatientIntakeAccessLink? link,
            DateTime utcNow)
        {
            return link is not null &&
                   link.Tenant is not null &&
                   link.Tenant.IsActive &&
                   (link.Branch is null || link.Branch.IsActive) &&
                   link.Purpose == PatientIntakeAccessLinkPurpose.NewPatientWaitingRoomRegistration &&
                   link.CanBeConsumedAt(utcNow);
        }
    }

    public sealed class PatientIntakeSessionService
        : IPatientIntakeSessionService
    {
        private readonly IPatientIntakeAuthenticationRepository _repository;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeSessionService(
            IPatientIntakeAuthenticationRepository repository,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<CurrentPatientIntakeSessionDto?> GetCurrentAsync(
            PatientIntakeSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            var state = await GetCurrentStateAsync(
                identity,
                trackAccountChanges: false,
                cancellationToken);
            if (state is null)
            {
                return null;
            }

            return new CurrentPatientIntakeSessionDto(
                state.Value.Account.Id,
                state.Value.Intake.Id,
                state.Value.Account.Tenant.Subdomain!,
                state.Value.Account.LoginName,
                state.Value.Account.SessionVersion);
        }

        public async Task<bool> RevokeCurrentSessionsAsync(
            PatientIntakeSessionIdentity identity,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var state = await GetCurrentStateAsync(
                identity,
                trackAccountChanges: true,
                cancellationToken);
            if (state is null)
            {
                return false;
            }

            var occurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            state.Value.Account.RevokeSessions(occurredAtUtc);
            var audit = new PatientIntakeAuthenticationAuditEntry(
                state.Value.Account,
                state.Value.Intake,
                PatientIntakeAuthenticationAuditAction.SessionsRevoked,
                PatientIntakeAuthenticationAuditActorType.PatientPortalAccount,
                state.Value.Account.Id,
                occurredAtUtc,
                correlationId);

            return await _repository.TrySaveAccountStateAsync(
                state.Value.Account,
                new[] { audit },
                cancellationToken);
        }

        private async Task<(PatientPortalAccount Account, PatientIntake Intake)?> GetCurrentStateAsync(
            PatientIntakeSessionIdentity identity,
            bool trackAccountChanges,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(identity);

            var account = await _repository.GetAccountForSessionAsync(
                identity.AccountId,
                identity.TenantId,
                trackAccountChanges,
                cancellationToken);
            if (account is null ||
                !account.IsActive ||
                account.PatientId.HasValue ||
                account.Tenant is null ||
                !account.Tenant.IsActive ||
                string.IsNullOrWhiteSpace(account.Tenant.Subdomain) ||
                account.SessionVersion != identity.SessionVersion)
            {
                return null;
            }

            var intake = await _repository.GetIntakeForSessionAsync(
                identity.IntakeId,
                identity.AccountId,
                identity.TenantId,
                trackChanges: false,
                cancellationToken);
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            if (intake is null ||
                intake.PatientId.HasValue ||
                intake.Origin != PatientIntakeOrigin.NewPatientWaitingRoom ||
                intake.Status != PatientIntakeStatus.Draft ||
                intake.IsExpiredAt(utcNow))
            {
                return null;
            }

            return (account, intake);
        }
    }
}
