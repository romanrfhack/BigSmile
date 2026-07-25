using BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientIntakeAccessLinks.Queries
{
    public interface IPatientIntakeAccessLinkQueryService
    {
        Task<IReadOnlyList<PatientIntakeAccessLinkSummaryDto>> ListAsync(
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientIntakeAccessLinkQueryService : IPatientIntakeAccessLinkQueryService
    {
        private const int MaximumReturnedAccessLinks = 100;

        private readonly IPatientIntakeAccessLinkRepository _accessLinkRepository;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeAccessLinkQueryService(
            IPatientIntakeAccessLinkRepository accessLinkRepository,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _accessLinkRepository = accessLinkRepository ?? throw new ArgumentNullException(nameof(accessLinkRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<IReadOnlyList<PatientIntakeAccessLinkSummaryDto>> ListAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureRequiredTenantContext();
            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var accessLinks = await _accessLinkRepository.ListAsync(
                MaximumReturnedAccessLinks,
                cancellationToken);

            return accessLinks
                .Select(accessLink => accessLink.ToSummaryDto(utcNow))
                .ToArray();
        }

        private void EnsureRequiredTenantContext()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() == AccessScope.Platform ||
                !Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) ||
                tenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake access link listing requires an authenticated resolved tenant without platform override.");
            }
        }
    }
}
