using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientPortalInvitations.Queries
{
    public interface IPatientPortalInvitationQueryService
    {
        Task<IReadOnlyList<PatientPortalInvitationSummaryDto>?> ListAsync(
            Guid patientId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientPortalInvitationQueryService : IPatientPortalInvitationQueryService
    {
        private const int MaximumReturnedInvitations = 50;

        private readonly IPatientPortalInvitationRepository _invitationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientPortalInvitationQueryService(
            IPatientPortalInvitationRepository invitationRepository,
            IPatientRepository patientRepository,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _invitationRepository = invitationRepository ?? throw new ArgumentNullException(nameof(invitationRepository));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<IReadOnlyList<PatientPortalInvitationSummaryDto>?> ListAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException("Patient portal invitations can only be listed inside the resolved tenant.");
            }

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var invitations = await _invitationRepository.ListByPatientIdAsync(
                patientId,
                MaximumReturnedInvitations,
                cancellationToken);

            return invitations
                .Select(invitation => invitation.ToSummaryDto(utcNow))
                .ToList();
        }

        private Guid GetRequiredTenantId()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                throw new InvalidOperationException(
                    "Patient portal invitation listing requires an authenticated tenant context without platform override.");
            }

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient portal invitation listing requires a resolved tenant.");
            }

            return tenantId;
        }
    }
}
