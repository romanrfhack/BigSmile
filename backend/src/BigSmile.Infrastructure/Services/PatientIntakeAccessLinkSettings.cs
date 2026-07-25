using BigSmile.Application.Interfaces.PatientIntakes;
using Microsoft.Extensions.Configuration;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientIntakeAccessLinkSettings
        : IPatientIntakeAccessLinkSettings
    {
        public const string LifetimeMinutesKey =
            "PatientPortal:Intake:WaitingRoomLinkLifetimeMinutes";
        public const int DefaultLifetimeMinutes = 30;
        public const int MinimumLifetimeMinutes = 5;
        public const int MaximumLifetimeMinutes = 120;

        public PatientIntakeAccessLinkSettings(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var configured = configuration[LifetimeMinutesKey];
            if (string.IsNullOrWhiteSpace(configured))
            {
                WaitingRoomLinkLifetime = TimeSpan.FromMinutes(
                    DefaultLifetimeMinutes);
                return;
            }

            if (!int.TryParse(configured, out var minutes) ||
                minutes is < MinimumLifetimeMinutes or > MaximumLifetimeMinutes)
            {
                throw new InvalidOperationException(
                    $"Configuration '{LifetimeMinutesKey}' must be an integer between " +
                    $"{MinimumLifetimeMinutes} and {MaximumLifetimeMinutes}.");
            }

            WaitingRoomLinkLifetime = TimeSpan.FromMinutes(minutes);
        }

        public TimeSpan WaitingRoomLinkLifetime { get; }
    }
}
