namespace BigSmile.Application.Interfaces.Repositories
{
    public sealed record DashboardSummaryCounts(
        int ActivePatientsCount,
        int TodayAppointmentsCount,
        int TodayPendingAppointmentsCount,
        int ActiveDocumentsCount,
        int ActiveTreatmentPlansCount,
        int AcceptedQuotesCount,
        int IssuedBillingDocumentsCount);

    public interface IDashboardSummaryRepository
    {
        Task<DashboardSummaryCounts> GetSummaryCountsAsync(
            Guid tenantId,
            DateTime todayStart,
            DateTime tomorrowStart,
            CancellationToken cancellationToken = default);
    }
}
