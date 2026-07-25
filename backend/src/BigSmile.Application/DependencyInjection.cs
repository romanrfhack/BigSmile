using BigSmile.Application.Authorization;
using BigSmile.Application.Features.BillingDocuments.Commands;
using BigSmile.Application.Features.BillingDocuments.Queries;
using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Branches.Queries;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using BigSmile.Application.Features.Dashboard.Queries;
using BigSmile.Application.Features.Odontograms.Commands;
using BigSmile.Application.Features.Odontograms.Queries;
using BigSmile.Application.Features.PatientDocuments.Commands;
using BigSmile.Application.Features.PatientDocuments.Queries;
using BigSmile.Application.Features.PatientIntakes.Services;
using BigSmile.Application.Features.PatientPortalAuthentication.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Queries;
using BigSmile.Application.Features.Patients.Commands;
using BigSmile.Application.Features.Patients.Queries;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Queries;
using BigSmile.Application.Features.Tenants.Queries;
using BigSmile.Application.Features.TreatmentPlans.Commands;
using BigSmile.Application.Features.TreatmentPlans.Queries;
using BigSmile.Application.Features.TreatmentQuotes.Commands;
using BigSmile.Application.Features.TreatmentQuotes.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace BigSmile.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton(TimeProvider.System);
            services.AddSingleton<IRolePermissionCatalog, RolePermissionCatalog>();
            services.AddScoped<IBillingDocumentCommandService, BillingDocumentCommandService>();
            services.AddScoped<IBillingDocumentQueryService, BillingDocumentQueryService>();
            services.AddScoped<IBranchAccessService, BranchAccessService>();
            services.AddScoped<IBranchQueryService, BranchQueryService>();
            services.AddScoped<IClinicalRecordCommandService, ClinicalRecordCommandService>();
            services.AddScoped<IClinicalRecordQueryService, ClinicalRecordQueryService>();
            services.AddScoped<IDashboardSummaryQueryService, DashboardSummaryQueryService>();
            services.AddScoped<IOdontogramCommandService, OdontogramCommandService>();
            services.AddScoped<IOdontogramQueryService, OdontogramQueryService>();
            services.AddScoped<IPatientDocumentCommandService, PatientDocumentCommandService>();
            services.AddScoped<IPatientDocumentQueryService, PatientDocumentQueryService>();
            services.AddScoped<IPatientIntakeSelfService, PatientIntakeSelfService>();
            services.AddScoped<IPatientPortalPublicAuthenticationService, PatientPortalPublicAuthenticationService>();
            services.AddScoped<IPatientPortalSessionService, PatientPortalSessionService>();
            services.AddScoped<IPatientPortalRecoveryService, PatientPortalRecoveryService>();
            services.AddScoped<IPatientPortalInvitationCommandService, PatientPortalInvitationCommandService>();
            services.AddScoped<IPatientPortalInvitationQueryService, PatientPortalInvitationQueryService>();
            services.AddScoped<IPatientCommandService, PatientCommandService>();
            services.AddScoped<IPatientQueryService, PatientQueryService>();
            services.AddScoped<IAppointmentCommandService, AppointmentCommandService>();
            services.AddScoped<IAppointmentBlockCommandService, AppointmentBlockCommandService>();
            services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
            services.AddScoped<IAppointmentReminderLogCommandService, AppointmentReminderLogCommandService>();
            services.AddScoped<IAppointmentReminderLogQueryService, AppointmentReminderLogQueryService>();
            services.AddScoped<IReminderTemplateCommandService, ReminderTemplateCommandService>();
            services.AddScoped<IReminderTemplateQueryService, ReminderTemplateQueryService>();
            services.AddScoped<ITenantQueryService, TenantQueryService>();
            services.AddScoped<ITreatmentPlanCommandService, TreatmentPlanCommandService>();
            services.AddScoped<ITreatmentPlanQueryService, TreatmentPlanQueryService>();
            services.AddScoped<ITreatmentQuoteCommandService, TreatmentQuoteCommandService>();
            services.AddScoped<ITreatmentQuoteQueryService, TreatmentQuoteQueryService>();

            return services;
        }
    }
}
