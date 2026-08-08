using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.PatientIntakeRequests.Dtos;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientIntakeRequests.Services
{
    public interface IAppointmentPatientIntakeRequestService
    {
        Task<AppointmentPatientIntakeRequestStatusDto?> GetStatusAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default);

        Task<AppointmentPatientIntakeRequestResult> PrepareAsync(
            Guid appointmentId,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class AppointmentPatientIntakeRequestService
        : IAppointmentPatientIntakeRequestService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IBranchAccessService _branchAccessService;
        private readonly IPatientPortalAuthenticationRepository _portalRepository;
        private readonly IPatientIntakeRepository _intakeRepository;
        private readonly IPatientPortalInvitationCommandService _invitationCommandService;
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public AppointmentPatientIntakeRequestService(
            IAppointmentRepository appointmentRepository,
            IBranchAccessService branchAccessService,
            IPatientPortalAuthenticationRepository portalRepository,
            IPatientIntakeRepository intakeRepository,
            IPatientPortalInvitationCommandService invitationCommandService,
            ITenantRepository tenantRepository,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
            _portalRepository = portalRepository ?? throw new ArgumentNullException(nameof(portalRepository));
            _intakeRepository = intakeRepository ?? throw new ArgumentNullException(nameof(intakeRepository));
            _invitationCommandService = invitationCommandService ?? throw new ArgumentNullException(nameof(invitationCommandService));
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<AppointmentPatientIntakeRequestStatusDto?> GetStatusAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var appointment = await GetAccessibleAppointmentAsync(
                appointmentId,
                tenantId,
                cancellationToken);
            if (appointment is null)
            {
                return null;
            }

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || string.IsNullOrWhiteSpace(tenant.Subdomain))
            {
                throw new InvalidOperationException(
                    "Patient intake requests require an active tenant portal realm.");
            }

            var account = await _portalRepository.GetAccountByPatientAsync(
                tenantId,
                appointment.PatientId,
                trackChanges: false,
                cancellationToken);
            var submittedIntake = await _intakeRepository.GetSubmittedByPatientAsync(
                tenantId,
                appointment.PatientId,
                trackChanges: false,
                cancellationToken);

            PatientIntake? activeDraft = null;
            if (account is not null && submittedIntake is null)
            {
                var draft = await _intakeRepository.GetDraftByAccountAsync(
                    tenantId,
                    account.Id,
                    trackChanges: false,
                    cancellationToken);
                var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
                if (draft is not null && !draft.IsExpiredAt(utcNow))
                {
                    activeDraft = draft;
                }
            }

            var portalAccessStatus = account is null
                ? "NotActivated"
                : account.IsActive
                    ? "Active"
                    : "RecoveryRequired";
            var intakeStatus = submittedIntake is not null
                ? "Completed"
                : activeDraft is not null
                    ? "InProgress"
                    : "NotStarted";
            var recommendedAccess = submittedIntake is not null
                ? "None"
                : account is null
                    ? "Activation"
                    : account.IsActive
                        ? "Login"
                        : "RecoveryRequired";
            var canRequest = appointment.Status == AppointmentStatus.Scheduled &&
                             appointment.Patient.IsActive &&
                             submittedIntake is null &&
                             (account is null || account.IsActive);

            return new AppointmentPatientIntakeRequestStatusDto(
                appointment.Id,
                appointment.PatientId,
                appointment.Patient.FullName,
                appointment.Patient.PrimaryPhone,
                tenant.Subdomain.Trim().ToLowerInvariant(),
                portalAccessStatus,
                intakeStatus,
                recommendedAccess,
                canRequest,
                submittedIntake?.SubmittedAtUtc);
        }

        public async Task<AppointmentPatientIntakeRequestResult> PrepareAsync(
            Guid appointmentId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var status = await GetStatusAsync(appointmentId, cancellationToken);
            if (status is null)
            {
                return AppointmentPatientIntakeRequestResult.Failed(
                    AppointmentPatientIntakeRequestFailure.NotFound);
            }

            if (status.IntakeStatus == "Completed")
            {
                return AppointmentPatientIntakeRequestResult.Failed(
                    AppointmentPatientIntakeRequestFailure.AlreadyCompleted);
            }

            if (status.RecommendedAccess == "RecoveryRequired")
            {
                return AppointmentPatientIntakeRequestResult.Failed(
                    AppointmentPatientIntakeRequestFailure.RecoveryRequired);
            }

            if (!status.CanRequest)
            {
                return AppointmentPatientIntakeRequestResult.Failed(
                    AppointmentPatientIntakeRequestFailure.Unavailable);
            }

            if (status.RecommendedAccess == "Login")
            {
                return AppointmentPatientIntakeRequestResult.Success(
                    new PreparedAppointmentPatientIntakeRequestDto(
                        status,
                        "Login",
                        ActivationToken: null));
            }

            var invitation = await _invitationCommandService.IssueAsync(
                status.PatientId,
                correlationId,
                cancellationToken);
            if (invitation is null)
            {
                return AppointmentPatientIntakeRequestResult.Failed(
                    AppointmentPatientIntakeRequestFailure.NotFound);
            }

            var preparedStatus = status with
            {
                RecommendedAccess = "Activation"
            };
            return AppointmentPatientIntakeRequestResult.Success(
                new PreparedAppointmentPatientIntakeRequestDto(
                    preparedStatus,
                    "Activation",
                    invitation.ActivationToken));
        }

        private async Task<Appointment?> GetAccessibleAppointmentAsync(
            Guid appointmentId,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(
                appointmentId,
                cancellationToken);
            if (appointment is null || appointment.TenantId != tenantId)
            {
                return null;
            }

            var branch = await _branchAccessService.GetAccessibleBranchAsync(
                appointment.BranchId,
                cancellationToken);
            return branch is null ? null : appointment;
        }

        private Guid GetRequiredTenantId()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() == AccessScope.Platform ||
                !Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) ||
                tenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake requests require an authenticated tenant context without platform override.");
            }

            return tenantId;
        }
    }
}
