using BigSmile.Application.Interfaces.Security;
using Microsoft.Extensions.Configuration;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalInvitationSettings : IPatientPortalInvitationSettings
    {
        public const string ConfigurationKey = "PatientPortal:Invitations:ExistingPatientActivationLifetimeHours";
        public const int DefaultExistingPatientActivationLifetimeHours = 24;
        public const int MaximumExistingPatientActivationLifetimeHours = 168;

        public PatientPortalInvitationSettings(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var configuredValue = configuration[ConfigurationKey];
            if (string.IsNullOrWhiteSpace(configuredValue))
            {
                ExistingPatientActivationLifetime = TimeSpan.FromHours(DefaultExistingPatientActivationLifetimeHours);
                return;
            }

            if (!int.TryParse(configuredValue, out var configuredHours) ||
                configuredHours <= 0 ||
                configuredHours > MaximumExistingPatientActivationLifetimeHours)
            {
                throw new InvalidOperationException(
                    $"Configuration '{ConfigurationKey}' must be an integer between 1 and {MaximumExistingPatientActivationLifetimeHours}.");
            }

            ExistingPatientActivationLifetime = TimeSpan.FromHours(configuredHours);
        }

        public TimeSpan ExistingPatientActivationLifetime { get; }
    }
}
