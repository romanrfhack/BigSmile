using BigSmile.Application.Features.Dashboard.Queries;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BigSmile.IntegrationTests.Dashboard
{
    public sealed class DashboardSummaryQueryServiceTests
    {
        [Fact]
        public async Task GetSummaryAsync_ReturnsTenantScopedOperationalCounts()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            await SeedTenantADashboardDataAsync(databaseName, tenantA, actorUserId);
            await SeedTenantBDashboardDataAsync(databaseName, tenantB, actorUserId);
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var summary = await queryService.GetSummaryAsync();

            Assert.Equal(2, summary.ActivePatientsCount);
            Assert.Equal(4, summary.TodayAppointmentsCount);
            Assert.Equal(1, summary.TodayPendingAppointmentsCount);
            Assert.Equal(1, summary.ActiveDocumentsCount);
            Assert.Equal(3, summary.ActiveTreatmentPlansCount);
            Assert.Equal(2, summary.AcceptedQuotesCount);
            Assert.Equal(1, summary.IssuedBillingDocumentsCount);
            Assert.True(summary.GeneratedAtUtc <= DateTime.UtcNow);
        }

        [Fact]
        public async Task GetSummaryAsync_UsesTodayByAppointmentStartDate()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var today = DateTime.UtcNow.Date;

            await SeedAppointmentAsync(databaseName, tenantA.Id, tenantA.Branches.Single().Id, patient.Id, today.AddHours(9), AppointmentStatus.Scheduled);
            await SeedAppointmentAsync(databaseName, tenantA.Id, tenantA.Branches.Single().Id, patient.Id, today.AddDays(-1).AddHours(9), AppointmentStatus.Scheduled);
            await SeedAppointmentAsync(databaseName, tenantA.Id, tenantA.Branches.Single().Id, patient.Id, today.AddDays(1).AddHours(9), AppointmentStatus.Scheduled);

            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);
            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var summary = await queryService.GetSummaryAsync();

            Assert.Equal(1, summary.TodayAppointmentsCount);
            Assert.Equal(1, summary.TodayPendingAppointmentsCount);
        }

        [Fact]
        public async Task GetSummaryAsync_BlocksPlatformScopeWithoutResolvedTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            await SeedTenantADashboardDataAsync(databaseName, tenantA, actorUserId);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(actorUserId.ToString(), AccessScope.Platform, isAuthenticated: true);

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.GetSummaryAsync());

            Assert.Contains("resolved tenant context", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetSummaryAsync_BlocksPlatformOverrideWithoutResolvedTenant()
        {
            var databaseName = Guid.NewGuid().ToString();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            await SeedTenantADashboardDataAsync(databaseName, tenantA, actorUserId);
            await SeedTenantBDashboardDataAsync(databaseName, tenantB, actorUserId);
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(actorUserId.ToString(), AccessScope.Platform, isAuthenticated: true);
            tenantContext.EnablePlatformOverride();

            await using var context = CreateContext(databaseName, tenantContext);
            var queryService = CreateQueryService(context, tenantContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => queryService.GetSummaryAsync());

            Assert.Contains("resolved tenant context", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        private static DashboardSummaryQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext)
        {
            return new DashboardSummaryQueryService(
                new EfDashboardSummaryRepository(context),
                tenantContext);
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(userId.ToString(), AccessScope.Tenant, isAuthenticated: true, tenantId.ToString());
            return tenantContext;
        }

        private static async Task<(Tenant TenantA, Tenant TenantB)> SeedTenantsAsync(string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            tenantA.AddBranch("Branch A");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            tenantB.AddBranch("Branch B");

            context.Tenants.AddRange(tenantA, tenantB);
            await context.SaveChangesAsync();

            return (tenantA, tenantB);
        }

        private static async Task SeedTenantADashboardDataAsync(string databaseName, Tenant tenant, Guid actorUserId)
        {
            var branchId = tenant.Branches.Single().Id;
            var today = DateTime.UtcNow.Date;
            var activePatientA = await SeedPatientAsync(databaseName, tenant.Id, "Ana", "Lopez");
            var activePatientB = await SeedPatientAsync(databaseName, tenant.Id, "Carla", "Ruiz");
            var inactivePatient = await SeedPatientAsync(databaseName, tenant.Id, "Inactive", "Patient", isActive: false);

            await SeedAppointmentAsync(databaseName, tenant.Id, branchId, activePatientA.Id, today.AddHours(9), AppointmentStatus.Scheduled);
            await SeedAppointmentAsync(databaseName, tenant.Id, branchId, activePatientA.Id, today.AddHours(10), AppointmentStatus.Attended);
            await SeedAppointmentAsync(databaseName, tenant.Id, branchId, activePatientB.Id, today.AddHours(11), AppointmentStatus.NoShow);
            await SeedAppointmentAsync(databaseName, tenant.Id, branchId, activePatientB.Id, today.AddHours(12), AppointmentStatus.Cancelled);
            await SeedAppointmentAsync(databaseName, tenant.Id, branchId, activePatientA.Id, today.AddDays(1).AddHours(9), AppointmentStatus.Scheduled);

            await SeedPatientDocumentAsync(databaseName, tenant.Id, activePatientA.Id, actorUserId, retire: false);
            await SeedPatientDocumentAsync(databaseName, tenant.Id, activePatientA.Id, actorUserId, retire: true);

            var acceptedQuoteA = await SeedTreatmentQuoteAsync(databaseName, tenant.Id, activePatientA.Id, actorUserId, TreatmentQuoteStatus.Accepted);
            var acceptedQuoteB = await SeedTreatmentQuoteAsync(databaseName, tenant.Id, activePatientB.Id, actorUserId, TreatmentQuoteStatus.Accepted);
            await SeedTreatmentQuoteAsync(databaseName, tenant.Id, inactivePatient.Id, actorUserId, TreatmentQuoteStatus.Draft);

            await SeedBillingDocumentAsync(databaseName, tenant.Id, activePatientA.Id, acceptedQuoteA, actorUserId, BillingDocumentStatus.Issued);
            await SeedBillingDocumentAsync(databaseName, tenant.Id, activePatientB.Id, acceptedQuoteB, actorUserId, BillingDocumentStatus.Draft);
        }

        private static async Task SeedTenantBDashboardDataAsync(string databaseName, Tenant tenant, Guid actorUserId)
        {
            var branchId = tenant.Branches.Single().Id;
            var today = DateTime.UtcNow.Date;
            var patient = await SeedPatientAsync(databaseName, tenant.Id, "Bruno", "Garcia");
            await SeedAppointmentAsync(databaseName, tenant.Id, branchId, patient.Id, today.AddHours(13), AppointmentStatus.Scheduled);
            await SeedPatientDocumentAsync(databaseName, tenant.Id, patient.Id, actorUserId, retire: false);
            var quote = await SeedTreatmentQuoteAsync(databaseName, tenant.Id, patient.Id, actorUserId, TreatmentQuoteStatus.Accepted);
            await SeedBillingDocumentAsync(databaseName, tenant.Id, patient.Id, quote, actorUserId, BillingDocumentStatus.Issued);
        }

        private static async Task<Patient> SeedPatientAsync(
            string databaseName,
            Guid tenantId,
            string firstName,
            string lastName,
            bool isActive = true)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var patient = new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14),
                "5551234567",
                $"{firstName.ToLowerInvariant()}@example.com",
                isActive);
            context.Patients.Add(patient);
            await context.SaveChangesAsync();

            return patient;
        }

        private static async Task SeedAppointmentAsync(
            string databaseName,
            Guid tenantId,
            Guid branchId,
            Guid patientId,
            DateTime startsAt,
            AppointmentStatus status)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var appointment = new Appointment(
                tenantId,
                branchId,
                patientId,
                startsAt,
                startsAt.AddMinutes(30),
                null);

            if (status == AppointmentStatus.Attended)
            {
                appointment.MarkAttended();
            }
            else if (status == AppointmentStatus.NoShow)
            {
                appointment.MarkNoShow();
            }
            else if (status == AppointmentStatus.Cancelled)
            {
                appointment.Cancel("Cancelled for dashboard test.");
            }

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPatientDocumentAsync(
            string databaseName,
            Guid tenantId,
            Guid patientId,
            Guid actorUserId,
            bool retire)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var document = new PatientDocument(
                tenantId,
                patientId,
                "radiography.pdf",
                "application/pdf",
                1024,
                $"documents/{Guid.NewGuid():N}.pdf",
                actorUserId);

            if (retire)
            {
                document.Retire(actorUserId);
            }

            context.PatientDocuments.Add(document);
            await context.SaveChangesAsync();
        }

        private static async Task<TreatmentQuote> SeedTreatmentQuoteAsync(
            string databaseName,
            Guid tenantId,
            Guid patientId,
            Guid actorUserId,
            TreatmentQuoteStatus status)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var treatmentPlan = new TreatmentPlan(tenantId, patientId, actorUserId);
            treatmentPlan.AddItem("Exam", "Diagnostics", 1, null, null, null, actorUserId);

            var treatmentQuote = new TreatmentQuote(
                tenantId,
                patientId,
                treatmentPlan.Id,
                treatmentPlan.Items,
                actorUserId);

            if (status is TreatmentQuoteStatus.Proposed or TreatmentQuoteStatus.Accepted)
            {
                treatmentQuote.UpdateItemUnitPrice(treatmentQuote.Items.Single().Id, 350m, actorUserId);
                treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Proposed, actorUserId);
            }

            if (status == TreatmentQuoteStatus.Accepted)
            {
                treatmentQuote.ChangeStatus(TreatmentQuoteStatus.Accepted, actorUserId);
            }

            context.TreatmentPlans.Add(treatmentPlan);
            context.TreatmentQuotes.Add(treatmentQuote);
            await context.SaveChangesAsync();

            return treatmentQuote;
        }

        private static async Task SeedBillingDocumentAsync(
            string databaseName,
            Guid tenantId,
            Guid patientId,
            TreatmentQuote treatmentQuote,
            Guid actorUserId,
            BillingDocumentStatus status)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var billingDocument = new BillingDocument(
                tenantId,
                patientId,
                treatmentQuote.Id,
                treatmentQuote.CurrencyCode,
                treatmentQuote.Items,
                actorUserId);

            if (status == BillingDocumentStatus.Issued)
            {
                billingDocument.ChangeStatus(BillingDocumentStatus.Issued, actorUserId);
            }

            context.BillingDocuments.Add(billingDocument);
            await context.SaveChangesAsync();
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var configuration = new ConfigurationBuilder().Build();
            return new AppDbContext(options, configuration, tenantContext);
        }
    }
}
