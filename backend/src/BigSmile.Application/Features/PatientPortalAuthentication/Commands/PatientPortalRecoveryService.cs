using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientPortalAuthentication.Commands
{
    public interface IPatientPortalRecoveryService
    {
        Task<IssuedPatientPortalInvitationDto?> StartRecoveryAsync(
            Guid patientId,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientPortalRecoveryService : IPatientPortalRecoveryService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientPortalAuthenticationRepository _authenticationRepository;
        private readonly IPatientPortalInvitationRepository _invitationRepository;
        private readonly IPatientPortalInvitationTokenService _tokenService;
        private readonly IPatientPortalInvitationSettings _invitationSettings;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientPortalRecoveryService(
            IPatientRepository patientRepository,
            IPatientPortalAuthenticationRepository authenticationRepository,
            IPatientPortalInvitationRepository invitationRepository,
            IPatientPortalInvitationTokenService tokenService,
            IPatientPortalInvitationSettings invitationSettings,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _authenticationRepository = authenticationRepository ?? throw new ArgumentNullException(nameof(authenticationRepository));
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _invitationSettings = invitationSettings ?? throw new ArgumentNullException(nameof(invitationSettings));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<IssuedPatientPortalInvitationDto?> StartRecoveryAsync(
            Guid patientId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var (tenantId, actorUserId) = GetRequiredStaffContext();
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException(
                    "Patient portal recovery can only target a patient from the resolved tenant.");
            }

            var account = await _authenticationRepository.GetAccountByPatientAsync(
                tenantId,
                patientId,
                trackChanges: true,
                cancellationToken);
            if (account is null)
            {
                return null;
            }

            var occurredAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            account.BeginRecovery(occurredAtUtc);

            var outstandingInvitations = await _invitationRepository.ListOutstandingByPatientIdAsync(
                patientId,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                cancellationToken);
            var invitationAuditEntries = new List<PatientPortalSecurityAuditEntry>(outstandingInvitations.Count + 1);

            foreach (var outstandingInvitation in outstandingInvitations)
            {
                outstandingInvitation.Revoke(actorUserId, occurredAtUtc);
                invitationAuditEntries.Add(new PatientPortalSecurityAuditEntry(
                    outstandingInvitation,
                    PatientPortalSecurityAuditAction.InvitationSuperseded,
                    actorUserId,
                    occurredAtUtc,
                    correlationId));
            }

            var generatedToken = _tokenService.Generate();
            var invitation = new PatientPortalInvitation(
                patient,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                generatedToken.TokenHash,
                occurredAtUtc,
                occurredAtUtc.Add(_invitationSettings.ExistingPatientActivationLifetime),
                actorUserId);
            invitationAuditEntries.Add(new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationIssued,
                actorUserId,
                occurredAtUtc,
                correlationId));

            var recoveryAuditEntry = new PatientPortalAuthenticationAuditEntry(
                account,
                PatientPortalAuthenticationAuditAction.RecoveryStarted,
                PatientPortalAuthenticationAuditActorType.StaffUser,
                actorUserId,
                occurredAtUtc,
                correlationId,
                invitation);

            await _authenticationRepository.SaveRecoveryAsync(
                account,
                invitation,
                outstandingInvitations,
                invitationAuditEntries,
                recoveryAuditEntry,
                cancellationToken);

            return new IssuedPatientPortalInvitationDto(
                invitation.Id,
                invitation.PatientId,
                invitation.Purpose.ToString(),
                generatedToken.RawToken,
                invitation.CreatedAtUtc,
                invitation.ExpiresAtUtc);
        }

        private (Guid TenantId, Guid ActorUserId) GetRequiredStaffContext()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() is AccessScope.Platform or AccessScope.Patient)
            {
                throw new InvalidOperationException(
                    "Patient portal recovery requires an authenticated tenant staff context without platform override.");
            }

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient portal recovery requires a resolved tenant.");
            }

            if (!Guid.TryParse(_tenantContext.GetUserId(), out var actorUserId) || actorUserId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient portal recovery requires a resolved staff actor.");
            }

            return (tenantId, actorUserId);
        }
    }
}
