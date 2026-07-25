using BigSmile.Application.Interfaces.PatientIntakes;
using BigSmile.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientIntakeDraftSettings : IPatientIntakeDraftSettings
    {
        public const string DraftLifetimeDaysKey = "PatientPortal:Intake:DraftLifetimeDays";
        public const int DefaultDraftLifetimeDays = 30;

        public PatientIntakeDraftSettings(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var configured = configuration[DraftLifetimeDaysKey];
            if (string.IsNullOrWhiteSpace(configured))
            {
                DraftLifetime = PatientIntake.DefaultDraftLifetime;
                return;
            }

            if (!int.TryParse(configured, out var days) ||
                days <= 0 ||
                days > PatientIntake.MaximumDraftLifetime.TotalDays)
            {
                throw new InvalidOperationException(
                    $"Configuration '{DraftLifetimeDaysKey}' must be an integer between 1 and " +
                    $"{PatientIntake.MaximumDraftLifetime.TotalDays:0}.");
            }

            DraftLifetime = TimeSpan.FromDays(days);
        }

        public TimeSpan DraftLifetime { get; }
    }
}
