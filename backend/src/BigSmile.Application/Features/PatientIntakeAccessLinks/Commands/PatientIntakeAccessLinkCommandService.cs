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
        Task<PatientIntakeAccessLinkIssueResult> IssueAsync(
            Guid? branchId,
            string correlationId,
            CancellationToken cancellationToken = default);

        Task<PatientIntakeAccessLinkRevokeResult> RevokeAsync(
            Guid linkId,
            string correlationId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientIntakeAccessLinkCommandService
        : IPatientIntakeAccessLinkCommandService
    {
        private readonly IPatientIntakeAccessLinkRepository _repository;
        private readonly IBranchRepository _branchRepository;
        private readonly IPatientIntakeAccessLinkTokenService _tokenService;
        private readonly IPatientIntakeAccessLinkSettings _settings;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeAccessLinkCommandService(
            IPatientIntakeAccessLinkRepository repository,
            IBranchRepository branchRepository,
            IPatientIntakeAccessLinkTokenService tokenService,
            IPatientIntakeAccessLinkSettings settings,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<PatientIntakeAccessLinkIssueResult> IssueAsync(
            Guid? branchId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            var (tenantId, actorUserId) = GetRequiredStaffContext();
            var branch = await GetOptionalBranchAsync(
                branchId,
                tenantId,
                cancellationToken);
            if (branchId.HasValue && branch is null)
            {
                return PatientIntakeAccessLinkIssueResult.Failed(
                    PatientIntakeAccessLinkIssueFailure.BranchUnavailable);
            }

            var issuedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            var generatedToken = _tokenService.Generate();
            var link = new PatientIntakeAccessLink(
                tenantId,
                branch,
                PatientIntakeAccessLinkPurpose.NewPatientWaitingRoomRegistration,
                generatedToken.TokenHash,
                issuedAtUtc,
                issuedAtUtc.Add(_settings.WaitingRoomLinkLifetime),
                actorUserId);
            var auditEntry = new PatientIntakeAccessLinkAuditEntry(
                link,
                PatientIntakeAccessLinkAuditAction.Issued,
                PatientIntakeAccessLinkAuditActorType.StaffUser,
                actorUserId,
                issuedAtUtc,
                correlationId);

            var saved = await _repository.TryIssueAsync(
                link,
                auditEntry,
                cancellationToken);
            if (!saved)
            {
                return PatientIntakeAccessLinkIssueResult.Failed(
                    PatientIntakeAccessLinkIssueFailure.ConcurrentConflict);
            }

            return PatientIntakeAccessLinkIssueResult.Success(
                new IssuedPatientIntakeAccessLinkDto(
                    link.Id,
                    link.BranchId,
                    link.Purpose.ToString(),
                    generatedToken.RawToken,
                    link.CreatedAtUtc,
                    link.ExpiresAtUtc));
        }

        public async Task<PatientIntakeAccessLinkRevokeResult> RevokeAsync(
            Guid linkId,
            string correlationId,
            CancellationToken cancellationToken = default)
        {
            if (linkId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Patient intake access link identifier is required.",
                    nameof(linkId));
            }

            var (tenantId, actorUserId) = GetRequiredStaffContext();
            var link = await _repository.GetByIdAsync(
                linkId,
                trackChanges: true,
                cancellationToken);
            if (link is null || link.TenantId != tenantId)
            {
                return PatientIntakeAccessLinkRevokeResult.Failed(
                    PatientIntakeAccessLinkRevokeFailure.Missing);
            }

            var revokedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
            if (!link.CanBeConsumedAt(revokedAtUtc))
            {
                return PatientIntakeAccessLinkRevokeResult.Failed(
                    PatientIntakeAccessLinkRevokeFailure.NotActive);
            }

            link.Revoke(actorUserId, revokedAtUtc);
            var auditEntry = new PatientIntakeAccessLinkAuditEntry(
                link,
                PatientIntakeAccessLinkAuditAction.Revoked,
                PatientIntakeAccessLinkAuditActorType.StaffUser,
                actorUserId,
                revokedAtUtc,
                correlationId);
            var saved = await _repository.TryRevokeAsync(
                link,
                auditEntry,
                cancellationToken);

            return saved
                ? PatientIntakeAccessLinkRevokeResult.Success()
                : PatientIntakeAccessLinkRevokeResult.Failed(
                    PatientIntakeAccessLinkRevokeFailure.ConcurrentConflict);
        }

        private async Task<Branch?> GetOptionalBranchAsync(
            Guid? branchId,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            if (!branchId.HasValue)
            {
                return null;
            }

            if (branchId.Value == Guid.Empty)
            {
                return null;
            }

            var branch = await _branchRepository.GetByIdAsync(
                branchId.Value,
                cancellationToken);
            return branch is not null &&
                   branch.TenantId == tenantId &&
                   branch.IsActive
                ? branch
                : null;
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

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) ||
                tenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake access link management requires a resolved tenant.");
            }

            if (!Guid.TryParse(_tenantContext.GetUserId(), out var actorUserId) ||
                actorUserId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake access link management requires a resolved staff actor.");
            }

            return (tenantId, actorUserId);
        }
    }
}
