using BigSmile.Application.Interfaces.Security;
using Microsoft.Extensions.Configuration;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalJwtSettings : IPatientPortalJwtSettings
    {
        public const string SecretKey = "PatientPortal:Jwt:Secret";
        public const string IssuerKey = "PatientPortal:Jwt:Issuer";
        public const string AudienceKey = "PatientPortal:Jwt:Audience";
        public const string AccessTokenLifetimeMinutesKey = "PatientPortal:Jwt:AccessTokenLifetimeMinutes";
        public const int DefaultAccessTokenLifetimeMinutes = 60;

        public PatientPortalJwtSettings(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            Secret = ReadRequired(configuration, SecretKey);
            if (Secret.Length < 32)
            {
                throw new InvalidOperationException(
                    $"Configuration '{SecretKey}' must contain at least 32 characters.");
            }

            Issuer = ReadRequired(configuration, IssuerKey);
            Audience = ReadRequired(configuration, AudienceKey);

            var configuredLifetime = configuration[AccessTokenLifetimeMinutesKey];
            var lifetimeMinutes = DefaultAccessTokenLifetimeMinutes;
            if (!string.IsNullOrWhiteSpace(configuredLifetime) &&
                (!int.TryParse(configuredLifetime, out lifetimeMinutes) || lifetimeMinutes is < 5 or > 1_440))
            {
                throw new InvalidOperationException(
                    $"Configuration '{AccessTokenLifetimeMinutesKey}' must be an integer between 5 and 1440.");
            }

            AccessTokenLifetime = TimeSpan.FromMinutes(lifetimeMinutes);
        }

        public string Secret { get; }
        public string Issuer { get; }
        public string Audience { get; }
        public TimeSpan AccessTokenLifetime { get; }

        private static string ReadRequired(IConfiguration configuration, string key)
        {
            var value = configuration[key]?.Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException($"Configuration '{key}' is required.");
            }

            return value;
        }
    }
}
