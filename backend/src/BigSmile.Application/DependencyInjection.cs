using BigSmile.Application.Authorization;
using BigSmile.Application.Features.BillingDocuments.Commands;
using BigSmile.Application.Features.BillingDocuments.Queries;
using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Branches.Queries;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using BigSmile.Application.Features.Odontograms.Commands;
using BigSmile.Application.Features.Odontograms.Queries;
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
            // Register application services (query handlers, command handlers, etc.)
            services.AddSingleton<IRolePermissionCatalog, RolePermissionCatalog>();
            services.AddScoped<IBillingDocumentCommandService, BillingDocumentCommandService>();
            services.AddScoped<IBillingDocumentQueryService, BillingDocumentQueryService>();
            services.AddScoped<IBranchAccessService, BranchAccessService>();
            services.AddScoped<IBranchQueryService, BranchQueryService>();
            services.AddScoped<IClinicalRecordCommandService, ClinicalRecordCommandService>();
            services.AddScoped<IClinicalRecordQueryService, ClinicalRecordQueryService>();
            services.AddScoped<IOdontogramCommandService, OdontogramCommandService>();
            services.AddScoped<IOdontogramQueryService, OdontogramQueryService>();
            services.AddScoped<IPatientCommandService, PatientCommandService>();
            services.AddScoped<IPatientQueryService, PatientQueryService>();
            services.AddScoped<IAppointmentCommandService, AppointmentCommandService>();
            services.AddScoped<IAppointmentBlockCommandService, AppointmentBlockCommandService>();
            services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
            services.AddScoped<ITenantQueryService, TenantQueryService>();
            services.AddScoped<ITreatmentPlanCommandService, TreatmentPlanCommandService>();
            services.AddScoped<ITreatmentPlanQueryService, TreatmentPlanQueryService>();
            services.AddScoped<ITreatmentQuoteCommandService, TreatmentQuoteCommandService>();
            services.AddScoped<ITreatmentQuoteQueryService, TreatmentQuoteQueryService>();

            return services;
        }
    }
}
