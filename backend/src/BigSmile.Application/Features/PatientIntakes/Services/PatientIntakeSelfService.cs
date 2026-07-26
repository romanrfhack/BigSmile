using BigSmile.Application.Features.PatientIntakes.Dtos;
using BigSmile.Application.Interfaces.PatientIntakes;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientIntakes.Services
{
    public interface IPatientIntakeSelfService
    {
        Task<PatientIntakeReadResult> GetCurrentAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeReadResult> GetCurrentAsync(
            PatientIntakeSessionIdentity identity,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeCreateResult> CreateAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeSaveResult> SaveAsync(
            PatientPortalSessionIdentity identity,
            SavePatientIntakeDraftCommand command,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeSaveResult> SaveAsync(
            PatientIntakeSessionIdentity identity,
            SavePatientIntakeDraftCommand command,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientIntakeSelfService : IPatientIntakeSelfService
    {
        private readonly IPatientPortalAuthenticationRepository _authenticationRepository;
        private readonly IPatientIntakeAuthenticationRepository? _intakeAuthenticationRepository;
        private readonly IPatientIntakeRepository _intakeRepository;
        private readonly IPatientIntakeDraftSettings _settings;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeSelfService(
            IPatientPortalAuthenticationRepository authenticationRepository,
            IPatientIntakeRepository intakeRepository,
            IPatientIntakeDraftSettings settings,
            TimeProvider timeProvider)
        {
            _authenticationRepository = authenticationRepository
                ?? throw new ArgumentNullException(nameof(authenticationRepository));
            _intakeAuthenticationRepository = null;
            _intakeRepository = intakeRepository
                ?? throw new ArgumentNullException(nameof(intakeRepository));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public PatientIntakeSelfService(
            IPatientPortalAuthenticationRepository authenticationRepository,
            IPatientIntakeAuthenticationRepository intakeAuthenticationRepository,
            IPatientIntakeRepository intakeRepository,
            IPatientIntakeDraftSettings settings,
            TimeProvider timeProvider)
        {
            _authenticationRepository = authenticationRepository
                ?? throw new ArgumentNullException(nameof(authenticationRepository));
            _intakeAuthenticationRepository = intakeAuthenticationRepository
                ?? throw new ArgumentNullException(nameof(intakeAuthenticationRepository));
            _intakeRepository = intakeRepository
                ?? throw new ArgumentNullException(nameof(intakeRepository));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<PatientIntakeReadResult> GetCurrentAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            var account = await GetCurrentAccountAsync(
                identity,
                trackChanges: false,
                cancellationToken);
            if (account is null)
            {
                return PatientIntakeReadResult.Failed(PatientIntakeReadFailure.SessionInvalid);
            }

            var intake = await _intakeRepository.GetDraftByAccountAsync(
                identity.TenantId,
                identity.AccountId,
                trackChanges: false,
                cancellationToken);
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            if (intake is null || intake.IsExpiredAt(utcNow))
            {
                return PatientIntakeReadResult.Failed(PatientIntakeReadFailure.Missing);
            }

            return PatientIntakeReadResult.Success(intake.ToDto());
        }

        public async Task<PatientIntakeReadResult> GetCurrentAsync(
            PatientIntakeSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            var state = await GetCurrentIntakeOnlyStateAsync(
                identity,
                trackIntakeChanges: false,
                cancellationToken);
            return state is null
                ? PatientIntakeReadResult.Failed(PatientIntakeReadFailure.SessionInvalid)
                : PatientIntakeReadResult.Success(state.Value.Intake.ToDto());
        }

        public async Task<PatientIntakeCreateResult> CreateAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default)
        {
            var account = await GetCurrentAccountAsync(
                identity,
                trackChanges: true,
                cancellationToken);
            if (account is null || account.Patient is null)
            {
                return PatientIntakeCreateResult.Failed(
                    PatientIntakeCreateFailure.SessionInvalid);
            }

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var currentDraft = await _intakeRepository.GetDraftByAccountAsync(
                identity.TenantId,
                identity.AccountId,
                trackChanges: true,
                cancellationToken);

            PatientIntake? expiredDraft = null;
            if (currentDraft is not null)
            {
                if (!currentDraft.IsExpiredAt(utcNow))
                {
                    return PatientIntakeCreateResult.Failed(
                        PatientIntakeCreateFailure.ActiveDraftExists);
                }

                currentDraft.ExpireIfDue(utcNow);
                expiredDraft = currentDraft;
            }

            var intake = PatientIntake.CreateForExistingPatient(
                account,
                account.Patient,
                branch: null,
                utcNow,
                _settings.DraftLifetime);

            var saved = await _intakeRepository.TryCreateAsync(
                intake,
                expiredDraft,
                cancellationToken);
            return saved
                ? PatientIntakeCreateResult.Success(intake.ToDto())
                : PatientIntakeCreateResult.Failed(
                    PatientIntakeCreateFailure.ConcurrentConflict);
        }

        public async Task<PatientIntakeSaveResult> SaveAsync(
            PatientPortalSessionIdentity identity,
            SavePatientIntakeDraftCommand command,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var account = await GetCurrentAccountAsync(
                identity,
                trackChanges: false,
                cancellationToken);
            if (account is null)
            {
                return PatientIntakeSaveResult.Failed(
                    PatientIntakeSaveFailure.SessionInvalid);
            }

            var intake = await _intakeRepository.GetDraftByAccountAsync(
                identity.TenantId,
                identity.AccountId,
                trackChanges: true,
                cancellationToken);
            return await SaveResolvedDraftAsync(
                intake,
                identity.AccountId,
                command,
                correlationId,
                cancellationToken);
        }

        public async Task<PatientIntakeSaveResult> SaveAsync(
            PatientIntakeSessionIdentity identity,
            SavePatientIntakeDraftCommand command,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var state = await GetCurrentIntakeOnlyStateAsync(
                identity,
                trackIntakeChanges: true,
                cancellationToken);
            if (state is null)
            {
                return PatientIntakeSaveResult.Failed(
                    PatientIntakeSaveFailure.SessionInvalid);
            }

            return await SaveResolvedDraftAsync(
                state.Value.Intake,
                identity.AccountId,
                command,
                correlationId,
                cancellationToken);
        }

        private async Task<PatientIntakeSaveResult> SaveResolvedDraftAsync(
            PatientIntake? intake,
            Guid actorAccountId,
            SavePatientIntakeDraftCommand command,
            string correlationId,
            CancellationToken cancellationToken)
        {
            if (intake is null)
            {
                return PatientIntakeSaveResult.Failed(PatientIntakeSaveFailure.Missing);
            }

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            if (intake.IsExpiredAt(utcNow))
            {
                intake.ExpireIfDue(utcNow);
                var expirationSaved = await _intakeRepository.TrySaveAsync(
                    intake,
                    cancellationToken);
                return PatientIntakeSaveResult.Failed(
                    expirationSaved
                        ? PatientIntakeSaveFailure.Expired
                        : PatientIntakeSaveFailure.ConcurrentConflict);
            }

            if (!PatientIntakeConcurrencyToken.Matches(
                    intake,
                    command.ConcurrencyToken))
            {
                return PatientIntakeSaveResult.Failed(
                    PatientIntakeSaveFailure.ConcurrentConflict);
            }

            var revision = intake.SaveDraft(
                command.ToDraftData(),
                actorAccountId,
                utcNow,
                correlationId,
                _settings.DraftLifetime);
            if (revision is null)
            {
                return PatientIntakeSaveResult.Success(
                    intake.ToDto(),
                    changed: false);
            }

            var saved = await _intakeRepository.TrySaveAsync(
                intake,
                cancellationToken);
            return saved
                ? PatientIntakeSaveResult.Success(intake.ToDto(), changed: true)
                : PatientIntakeSaveResult.Failed(
                    PatientIntakeSaveFailure.ConcurrentConflict);
        }

        private async Task<PatientPortalAccount?> GetCurrentAccountAsync(
            PatientPortalSessionIdentity identity,
            bool trackChanges,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(identity);

            var account = await _authenticationRepository.GetAccountForSessionAsync(
                identity.AccountId,
                identity.TenantId,
                identity.PatientId,
                trackChanges,
                cancellationToken);

            return IsCurrentSession(account, identity) ? account : null;
        }

        private async Task<(PatientPortalAccount Account, PatientIntake Intake)?> GetCurrentIntakeOnlyStateAsync(
            PatientIntakeSessionIdentity identity,
            bool trackIntakeChanges,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(identity);

            if (_intakeAuthenticationRepository is null)
            {
                throw new InvalidOperationException(
                    "Intake-only session support is not configured for this service instance.");
            }

            var account = await _intakeAuthenticationRepository.GetAccountForSessionAsync(
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
                return null;
            }

            var intake = await _intakeAuthenticationRepository.GetIntakeForSessionAsync(
                identity.IntakeId,
                identity.AccountId,
                identity.TenantId,
                trackIntakeChanges,
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

        private static bool IsCurrentSession(
            PatientPortalAccount? account,
            PatientPortalSessionIdentity identity)
        {
            return account is not null &&
                   account.IsActive &&
                   account.Tenant is not null &&
                   account.Tenant.IsActive &&
                   account.Patient is not null &&
                   account.Patient.IsActive &&
                   account.PatientId == identity.PatientId &&
                   account.TenantId == identity.TenantId &&
                   account.SessionVersion == identity.SessionVersion;
        }
    }
}
