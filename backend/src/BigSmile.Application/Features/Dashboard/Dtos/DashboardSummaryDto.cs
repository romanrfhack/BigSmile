namespace BigSmile.Application.Features.Dashboard.Dtos
{
    public sealed record DashboardSummaryDto(
        int ActivePatientsCount,
        int TodayAppointmentsCount,
        int TodayPendingAppointmentsCount,
        int ActiveDocumentsCount,
        int ActiveTreatmentPlansCount,
        int AcceptedQuotesCount,
        int IssuedBillingDocumentsCount,
        DateTime GeneratedAtUtc);
}
