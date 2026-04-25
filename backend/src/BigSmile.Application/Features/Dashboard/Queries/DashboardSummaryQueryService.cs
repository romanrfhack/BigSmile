using BigSmile.Application.Features.Dashboard.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Dashboard.Queries
{
    public interface IDashboardSummaryQueryService
    {
        Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
    }

    public sealed class DashboardSummaryQueryService : IDashboardSummaryQueryService
    {
        private readonly IDashboardSummaryRepository _dashboardSummaryRepository;
        private readonly ITenantContext _tenantContext;

        public DashboardSummaryQueryService(
            IDashboardSummaryRepository dashboardSummaryRepository,
            ITenantContext tenantContext)
        {
            _dashboardSummaryRepository = dashboardSummaryRepository ?? throw new ArgumentNullException(nameof(dashboardSummaryRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = ResolveTenantId();
            var generatedAtUtc = DateTime.UtcNow;
            var todayStartUtc = generatedAtUtc.Date;
            var tomorrowStartUtc = todayStartUtc.AddDays(1);

            var counts = await _dashboardSummaryRepository.GetSummaryCountsAsync(
                tenantId,
                todayStartUtc,
                tomorrowStartUtc,
                cancellationToken);

            return new DashboardSummaryDto(
                counts.ActivePatientsCount,
                counts.TodayAppointmentsCount,
                counts.TodayPendingAppointmentsCount,
                counts.ActiveDocumentsCount,
                counts.ActiveTreatmentPlansCount,
                counts.AcceptedQuotesCount,
                counts.IssuedBillingDocumentsCount,
                generatedAtUtc);
        }

        private Guid ResolveTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Dashboard summary requires a resolved tenant context.");
            }

            return tenantId;
        }
    }
}
