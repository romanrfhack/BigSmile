using BigSmile.Application.Interfaces.PatientIntakes;
using Microsoft.Extensions.Configuration;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientIntakeAccessLinkSettings : IPatientIntakeAccessLinkSettings
    {
        public const string WaitingRoomLinkLifetimeMinutesKey =
            "PatientPortal:Intake:WaitingRoomLinkLifetimeMinutes";
        public const int DefaultWaitingRoomLinkLifetimeMinutes = 30;
        public const int MinimumWaitingRoomLinkLifetimeMinutes = 5;
        public const int MaximumWaitingRoomLinkLifetimeMinutes = 120;

        public PatientIntakeAccessLinkSettings(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            var configured = configuration[WaitingRoomLinkLifetimeMinutesKey];
            if (string.IsNullOrWhiteSpace(configured))
            {
                WaitingRoomLinkLifetime = TimeSpan.FromMinutes(
                    DefaultWaitingRoomLinkLifetimeMinutes);
                return;
            }

            if (!int.TryParse(configured, out var minutes) ||
                minutes < MinimumWaitingRoomLinkLifetimeMinutes ||
                minutes > MaximumWaitingRoomLinkLifetimeMinutes)
            {
                throw new InvalidOperationException(
                    $"Configuration '{WaitingRoomLinkLifetimeMinutesKey}' must be an integer between " +
                    $"{MinimumWaitingRoomLinkLifetimeMinutes} and {MaximumWaitingRoomLinkLifetimeMinutes}.");
            }

            WaitingRoomLinkLifetime = TimeSpan.FromMinutes(minutes);
        }

        public TimeSpan WaitingRoomLinkLifetime { get; }
    }
}
