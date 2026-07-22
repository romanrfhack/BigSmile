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
        private readonly ITenantRepository _tenantRepository;
        private readonly ITenantContext _tenantContext;
        private readonly TimeProvider _timeProvider;

        public DashboardSummaryQueryService(
            IDashboardSummaryRepository dashboardSummaryRepository,
            ITenantRepository tenantRepository,
            ITenantContext tenantContext,
            TimeProvider timeProvider)
        {
            _dashboardSummaryRepository = dashboardSummaryRepository ?? throw new ArgumentNullException(nameof(dashboardSummaryRepository));
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = ResolveTenantId();
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken)
                ?? throw new InvalidOperationException("Dashboard summary tenant is not available in the resolved tenant context.");

            var generatedAtUtc = _timeProvider.GetUtcNow();
            var tenantTimeZone = ResolveTimeZone(tenant.TimeZoneId);
            var tenantNow = TimeZoneInfo.ConvertTime(generatedAtUtc, tenantTimeZone);
            var tenantToday = DateOnly.FromDateTime(tenantNow.DateTime);
            var todayStart = tenantToday.ToDateTime(TimeOnly.MinValue);
            var tomorrowStart = tenantToday.AddDays(1).ToDateTime(TimeOnly.MinValue);

            var counts = await _dashboardSummaryRepository.GetSummaryCountsAsync(
                tenantId,
                todayStart,
                tomorrowStart,
                cancellationToken);

            return new DashboardSummaryDto(
                counts.ActivePatientsCount,
                counts.TodayAppointmentsCount,
                counts.TodayPendingAppointmentsCount,
                counts.ActiveDocumentsCount,
                counts.ActiveTreatmentPlansCount,
                counts.AcceptedQuotesCount,
                counts.IssuedBillingDocumentsCount,
                generatedAtUtc.UtcDateTime);
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

        private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException exception)
            {
                throw new InvalidOperationException(
                    $"Dashboard summary cannot resolve tenant time zone '{timeZoneId}'.",
                    exception);
            }
            catch (InvalidTimeZoneException exception)
            {
                throw new InvalidOperationException(
                    $"Dashboard summary cannot use invalid tenant time zone '{timeZoneId}'.",
                    exception);
            }
        }
    }
}
