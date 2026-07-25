using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientPortalInvitations.Commands
{
    public interface IPatientPortalInvitationCommandService
    {
        Task<IssuedPatientPortalInvitationDto?> IssueAsync(
            Guid patientId,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task<bool> RevokeAsync(
            Guid patientId,
            Guid invitationId,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientPortalInvitationCommandService : IPatientPortalInvitationCommandService
    {
        private readonly IPatientPortalInvitationRepository _invitationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientPortalInvitationTokenService _tokenService;
        private readonly IPatientPortalInvitationSettings _settings;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientPortalInvitationCommandService(
            IPatientPortalInvitationRepository invitationRepository,
            IPatientRepository patientRepository,
            IPatientPortalInvitationTokenService tokenService,
            IPatientPortalInvitationSettings settings,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<IssuedPatientPortalInvitationDto?> IssueAsync(
            Guid patientId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var (tenantId, actorUserId) = GetRequiredStaffContext();
            var patient = await GetPatientInsideTenantAsync(patientId, tenantId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var issuedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            var outstandingInvitations = await _invitationRepository.ListOutstandingByPatientIdAsync(
                patientId,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                cancellationToken);

            var supersededAuditEntries = new List<PatientPortalSecurityAuditEntry>(outstandingInvitations.Count);
            foreach (var outstandingInvitation in outstandingInvitations)
            {
                outstandingInvitation.Revoke(actorUserId, issuedAtUtc);
                supersededAuditEntries.Add(new PatientPortalSecurityAuditEntry(
                    outstandingInvitation,
                    PatientPortalSecurityAuditAction.InvitationSuperseded,
                    actorUserId,
                    issuedAtUtc,
                    correlationId));
            }

            var generatedToken = _tokenService.Generate();
            var invitation = new PatientPortalInvitation(
                patient,
                PatientPortalInvitationPurpose.ExistingPatientActivation,
                generatedToken.TokenHash,
                issuedAtUtc,
                issuedAtUtc.Add(_settings.ExistingPatientActivationLifetime),
                actorUserId);
            var issuedAuditEntry = new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationIssued,
                actorUserId,
                issuedAtUtc,
                correlationId);

            await _invitationRepository.SaveIssueAsync(
                invitation,
                outstandingInvitations,
                supersededAuditEntries,
                issuedAuditEntry,
                cancellationToken);

            return new IssuedPatientPortalInvitationDto(
                invitation.Id,
                invitation.PatientId,
                invitation.Purpose.ToString(),
                generatedToken.RawToken,
                invitation.CreatedAtUtc,
                invitation.ExpiresAtUtc);
        }

        public async Task<bool> RevokeAsync(
            Guid patientId,
            Guid invitationId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var (tenantId, actorUserId) = GetRequiredStaffContext();
            var patient = await GetPatientInsideTenantAsync(patientId, tenantId, cancellationToken);
            if (patient is null)
            {
                return false;
            }

            var invitation = await _invitationRepository.GetByIdAsync(patientId, invitationId, cancellationToken);
            if (invitation is null)
            {
                return false;
            }

            var revokedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            invitation.Revoke(actorUserId, revokedAtUtc);
            var auditEntry = new PatientPortalSecurityAuditEntry(
                invitation,
                PatientPortalSecurityAuditAction.InvitationRevoked,
                actorUserId,
                revokedAtUtc,
                correlationId);

            await _invitationRepository.SaveRevocationAsync(invitation, auditEntry, cancellationToken);
            return true;
        }

        private async Task<Patient?> GetPatientInsideTenantAsync(
            Guid patientId,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException("Patient portal invitations can only target patients from the resolved tenant.");
            }

            return patient;
        }

        private (Guid TenantId, Guid ActorUserId) GetRequiredStaffContext()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                throw new InvalidOperationException(
                    "Patient portal invitation management requires an authenticated tenant context without platform override.");
            }

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient portal invitation management requires a resolved tenant.");
            }

            if (!Guid.TryParse(_tenantContext.GetUserId(), out var actorUserId) || actorUserId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient portal invitation management requires a resolved staff actor.");
            }

            return (tenantId, actorUserId);
        }
    }
}
