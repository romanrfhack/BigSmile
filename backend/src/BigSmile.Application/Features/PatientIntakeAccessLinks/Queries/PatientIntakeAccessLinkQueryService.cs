using BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientIntakeAccessLinks.Queries
{
    public interface IPatientIntakeAccessLinkQueryService
    {
        Task<IReadOnlyList<PatientIntakeAccessLinkSummaryDto>> ListAsync(
            bool includeResolved,
            int take,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientIntakeAccessLinkQueryService
        : IPatientIntakeAccessLinkQueryService
    {
        public const int MaximumReturnedLinks = 100;

        private readonly IPatientIntakeAccessLinkRepository _repository;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public PatientIntakeAccessLinkQueryService(
            IPatientIntakeAccessLinkRepository repository,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<IReadOnlyList<PatientIntakeAccessLinkSummaryDto>> ListAsync(
            bool includeResolved,
            int take,
            CancellationToken cancellationToken = default)
        {
            _ = GetRequiredTenantId();

            if (take is < 1 or > MaximumReturnedLinks)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(take),
                    $"Patient intake access link list size must be between 1 and {MaximumReturnedLinks}.");
            }

            var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
            var links = await _repository.ListAsync(
                utcNow,
                includeResolved,
                take,
                cancellationToken);

            return links
                .Select(link => link.ToSummaryDto(utcNow))
                .ToArray();
        }

        private Guid GetRequiredTenantId()
        {
            if (!_tenantContext.IsAuthenticated() ||
                _tenantContext.HasPlatformOverride() ||
                _tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                throw new InvalidOperationException(
                    "Patient intake access link listing requires an authenticated tenant context without platform override.");
            }

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var tenantId) ||
                tenantId == Guid.Empty)
            {
                throw new InvalidOperationException(
                    "Patient intake access link listing requires a resolved tenant.");
            }

            return tenantId;
        }
    }
}
