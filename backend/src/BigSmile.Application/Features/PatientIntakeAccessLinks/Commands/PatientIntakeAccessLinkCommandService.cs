using BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos;
using BigSmile.Application.Interfaces.PatientIntakes;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientIntakeAccessLinks.Commands
{
    public interface IPatientIntakeAccessLinkCommandService
    {
        Task<IssuedPatientIntakeAccessLinkDto?> IssueAsync(
            Guid? branchId,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task<bool> RevokeAsync(
            Guid accessLinkId,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientIntakeAccessLinkCommandService : IPatientIntakeAccessLinkCommandService
    {
        private readonly IPatientIntakeAccessLinkRepository _accessLinkRepository;
        private readonly ITenantRepository _tenantRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IPatientIntakeAccessLinkTokenService _tokenService;
        private readonly IPatientIntakeAccessLinkSettings _settings;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeAccessLinkCommandService(
            IPatientIntakeAccessLinkRepository accessLinkRepository,
            ITenantRepository tenantRepository,
            IBranchRepository branchRepository,
            IPatientIntakeAccessLinkTokenService tokenService,
            IPatientIntakeAccessLinkSettings settings,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _accessLinkRepository = accessLinkRepository ?? throw new ArgumentNullException(nameof(accessLinkRepository));
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<IssuedPatientIntakeAccessLinkDto?> IssueAsync(
            Guid? branchId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var (tenantId, actorUserId) = GetRequiredStaffContext();
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant is null || !tenant.IsActive)
            {
                throw new InvalidOperationException(
                    "Patient intake access link management requires an active resolved tenant.");
            }

            Branch? branch = null;
            if (branchId.HasValue)
            {
                branch = await _branchRepository.GetByIdAsync(branchId.Value, cancellationToken);
                if (branch is null || !branch.IsActive)
                {
                    return null;
                }

                if (branch.TenantId != tenantId)
                {
                    throw new InvalidOperationException(
                        "Patient intake access link Branch must belong to the resolved tenant.");
                }
            }

            var createdAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            var generatedToken = _tokenService.Generate();
            var accessLink = new PatientIntakeAccessLink(
                tenant,
                branch,
                generatedToken.TokenHash,
                createdAtUtc,
                createdAtUtc.Add(_settings.WaitingRoomLinkLifetime),
                actorUserId);
            var auditEntry = new PatientIntakeAccessLinkAuditEntry(
                accessLink,
                PatientIntakeAccessLinkAuditAction.Issued,
                PatientIntakeAccessLinkAuditActorType.StaffUser,
                actorUserId,
                createdAtUtc,
                correlationId);

            await _accessLinkRepository.SaveIssueAsync(
                accessLink,
                auditEntry,
                cancellationToken);

            return new IssuedPatientIntakeAccessLinkDto(
                accessLink.Id,
                accessLink.BranchId,
                branch?.Name,
                generatedToken.RawToken,
                accessLink.CreatedAtUtc,
                accessLink.ExpiresAtUtc);
        }

        public async Task<bool> RevokeAsync(
            Guid accessLinkId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var (_, actorUserId) = GetRequiredStaffContext();
            var accessLink = await _accessLinkRepository.GetByIdAsync(
                accessLinkId,
                trackChanges: true,
                cancellationToken);
            if (accessLink is null)
            {
                return false;
            }

            var revokedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            accessLink.Revoke(actorUserId, revokedAtUtc);
            var auditEntry = new PatientIntakeAccessLinkAuditEntry(
                accessLink,
                PatientIntakeAccessLinkAuditAction.Revoked,
                PatientIntakeAccessLinkAuditActorType.StaffUser,
                actorUserId,
                revokedAtUtc,
                correlationId);

            await _accessLinkRepository.SaveRevocationAsync(
                accessLink,
                auditEntry,
                cancellationToken);
            return true;
        }

        private (Guid TenantId, Guid ActorUserId) GetRequiredStaffContext()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                throw new InvalidOperationException(
                    "Patient intake access link management requires an authenticated tenant context without platform override.");
            }

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake access link management requires a resolved tenant.");
            }

            if (!Guid.TryParse(_tenantContext.GetUserId(), out var actorUserId) || actorUserId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake access link management requires a resolved staff actor.");
            }

            return (tenantId, actorUserId);
        }
    }
}
