using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public sealed class EfDashboardSummaryRepository : IDashboardSummaryRepository
    {
        private readonly AppDbContext _dbContext;

        public EfDashboardSummaryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<DashboardSummaryCounts> GetSummaryCountsAsync(
            Guid tenantId,
            DateTime todayStart,
            DateTime tomorrowStart,
            CancellationToken cancellationToken = default)
        {
            var activePatientsCount = await _dbContext.Patients
                .CountAsync(patient => patient.TenantId == tenantId && patient.IsActive, cancellationToken);

            var todayAppointmentsCount = await _dbContext.Appointments
                .CountAsync(appointment =>
                    appointment.TenantId == tenantId &&
                    appointment.StartsAt >= todayStart &&
                    appointment.StartsAt < tomorrowStart,
                    cancellationToken);

            var todayPendingAppointmentsCount = await _dbContext.Appointments
                .CountAsync(appointment =>
                    appointment.TenantId == tenantId &&
                    appointment.StartsAt >= todayStart &&
                    appointment.StartsAt < tomorrowStart &&
                    appointment.Status == AppointmentStatus.Scheduled,
                    cancellationToken);

            var activeDocumentsCount = await _dbContext.PatientDocuments
                .CountAsync(document =>
                    document.TenantId == tenantId &&
                    document.DeletedAtUtc == null,
                    cancellationToken);

            var activeTreatmentPlansCount = await _dbContext.TreatmentPlans
                .CountAsync(treatmentPlan => treatmentPlan.TenantId == tenantId, cancellationToken);

            var acceptedQuotesCount = await _dbContext.TreatmentQuotes
                .CountAsync(treatmentQuote =>
                    treatmentQuote.TenantId == tenantId &&
                    treatmentQuote.Status == TreatmentQuoteStatus.Accepted,
                    cancellationToken);

            var issuedBillingDocumentsCount = await _dbContext.BillingDocuments
                .CountAsync(billingDocument =>
                    billingDocument.TenantId == tenantId &&
                    billingDocument.Status == BillingDocumentStatus.Issued,
                    cancellationToken);

            return new DashboardSummaryCounts(
                activePatientsCount,
                todayAppointmentsCount,
                todayPendingAppointmentsCount,
                activeDocumentsCount,
                activeTreatmentPlansCount,
                acceptedQuotesCount,
                issuedBillingDocumentsCount);
        }
    }
}
