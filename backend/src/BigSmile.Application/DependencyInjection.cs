using BigSmile.Application.Authorization;
using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Features.Branches.Queries;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using BigSmile.Application.Features.Patients.Commands;
using BigSmile.Application.Features.Patients.Queries;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Queries;
using BigSmile.Application.Features.Tenants.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace BigSmile.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register application services (query handlers, command handlers, etc.)
            services.AddSingleton<IRolePermissionCatalog, RolePermissionCatalog>();
            services.AddScoped<IBranchAccessService, BranchAccessService>();
            services.AddScoped<IBranchQueryService, BranchQueryService>();
            services.AddScoped<IClinicalRecordCommandService, ClinicalRecordCommandService>();
            services.AddScoped<IClinicalRecordQueryService, ClinicalRecordQueryService>();
            services.AddScoped<IPatientCommandService, PatientCommandService>();
            services.AddScoped<IPatientQueryService, PatientQueryService>();
            services.AddScoped<IAppointmentCommandService, AppointmentCommandService>();
            services.AddScoped<IAppointmentBlockCommandService, AppointmentBlockCommandService>();
            services.AddScoped<IAppointmentQueryService, AppointmentQueryService>();
            services.AddScoped<ITenantQueryService, TenantQueryService>();

            return services;
        }
    }
}
