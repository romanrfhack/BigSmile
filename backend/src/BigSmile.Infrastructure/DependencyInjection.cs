using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Application.Interfaces.Storage;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BigSmile.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var patientDocumentStorageOptions = new PatientDocumentStorageOptions
            {
                RootPath = configuration[$"{PatientDocumentStorageOptions.SectionName}:RootPath"]
            };

            services.AddSingleton<IOptions<PatientDocumentStorageOptions>>(
                Microsoft.Extensions.Options.Options.Create(patientDocumentStorageOptions));

            services.AddSingleton<IPatientPortalAuthenticationSettings>(
                _ => new PatientPortalAuthenticationSettings(configuration));
            services.AddSingleton<IPatientPortalJwtSettings>(
                _ => new PatientPortalJwtSettings(configuration));

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ITenantRepository, EfTenantRepository>();
            services.AddScoped<IBranchRepository, EfBranchRepository>();
            services.AddScoped<IPatientRepository, EfPatientRepository>();
            services.AddScoped<IClinicalRecordRepository, EfClinicalRecordRepository>();
            services.AddScoped<IOdontogramRepository, EfOdontogramRepository>();
            services.AddScoped<IPatientDocumentRepository, EfPatientDocumentRepository>();
            services.AddScoped<IPatientPortalInvitationRepository, EfPatientPortalInvitationRepository>();
            services.AddScoped<IPatientPortalAuthenticationRepository, EfPatientPortalAuthenticationRepository>();
            services.AddScoped<IDashboardSummaryRepository, EfDashboardSummaryRepository>();
            services.AddScoped<IBillingDocumentRepository, EfBillingDocumentRepository>();
            services.AddScoped<ITreatmentPlanRepository, EfTreatmentPlanRepository>();
            services.AddScoped<ITreatmentQuoteRepository, EfTreatmentQuoteRepository>();
            services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
            services.AddScoped<IAppointmentBlockRepository, EfAppointmentBlockRepository>();
            services.AddScoped<IAppointmentReminderLogRepository, EfAppointmentReminderLogRepository>();
            services.AddScoped<IReminderTemplateRepository, EfReminderTemplateRepository>();
            services.AddScoped<IUserRepository, EfUserRepository>();
            services.AddScoped<IRoleRepository, EfRoleRepository>();
            services.AddScoped<IUserTenantMembershipRepository, EfUserTenantMembershipRepository>();
            services.AddScoped<IUserBranchAssignmentStore, EfUserBranchAssignmentStore>();
            services.AddScoped<RealPilotUserBootstrapper>();

            services.AddScoped<TenantContext>();
            services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            services.AddSingleton<IPatientPortalInvitationTokenService, PatientPortalInvitationTokenService>();
            services.AddSingleton<IPatientPortalInvitationSettings, PatientPortalInvitationSettings>();
            services.AddSingleton<IPatientPortalPasswordHasher, PatientPortalPasswordHasher>();
            services.AddSingleton<IPatientPortalJwtTokenService, PatientPortalJwtTokenService>();
            services.AddScoped<IPatientPortalSessionValidator, PatientPortalSessionValidator>();

            services.AddScoped<IPatientDocumentBinaryStore, LocalPatientDocumentBinaryStore>();

            return services;
        }
    }
}
