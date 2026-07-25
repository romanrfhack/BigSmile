using BigSmile.Application.Interfaces.Security;
using Microsoft.Extensions.Configuration;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalAuthenticationSettings : IPatientPortalAuthenticationSettings
    {
        public const string PasswordHashIterationCountKey = "PatientPortal:Authentication:PasswordHashIterationCount";
        public const string MinimumPasswordLengthKey = "PatientPortal:Authentication:MinimumPasswordLength";
        public const string MaximumPasswordLengthKey = "PatientPortal:Authentication:MaximumPasswordLength";
        public const string MaximumFailedLoginAttemptsKey = "PatientPortal:Authentication:MaximumFailedLoginAttempts";
        public const string LockoutDurationMinutesKey = "PatientPortal:Authentication:LockoutDurationMinutes";

        public const int DefaultPasswordHashIterationCount = 100_000;
        public const int DefaultMinimumPasswordLength = 12;
        public const int DefaultMaximumPasswordLength = 128;
        public const int DefaultMaximumFailedLoginAttempts = 5;
        public const int DefaultLockoutDurationMinutes = 15;

        public PatientPortalAuthenticationSettings(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            PasswordHashIterationCount = ReadInt(
                configuration,
                PasswordHashIterationCountKey,
                DefaultPasswordHashIterationCount,
                minimum: 100_000,
                maximum: 2_000_000);
            MinimumPasswordLength = ReadInt(
                configuration,
                MinimumPasswordLengthKey,
                DefaultMinimumPasswordLength,
                minimum: 10,
                maximum: 64);
            MaximumPasswordLength = ReadInt(
                configuration,
                MaximumPasswordLengthKey,
                DefaultMaximumPasswordLength,
                minimum: MinimumPasswordLength,
                maximum: 512);
            MaximumFailedLoginAttempts = ReadInt(
                configuration,
                MaximumFailedLoginAttemptsKey,
                DefaultMaximumFailedLoginAttempts,
                minimum: 1,
                maximum: 20);
            var lockoutMinutes = ReadInt(
                configuration,
                LockoutDurationMinutesKey,
                DefaultLockoutDurationMinutes,
                minimum: 1,
                maximum: 1_440);
            LockoutDuration = TimeSpan.FromMinutes(lockoutMinutes);
        }

        public int PasswordHashIterationCount { get; }
        public int MinimumPasswordLength { get; }
        public int MaximumPasswordLength { get; }
        public int MaximumFailedLoginAttempts { get; }
        public TimeSpan LockoutDuration { get; }

        private static int ReadInt(
            IConfiguration configuration,
            string key,
            int defaultValue,
            int minimum,
            int maximum)
        {
            var configured = configuration[key];
            if (string.IsNullOrWhiteSpace(configured))
            {
                return defaultValue;
            }

            if (!int.TryParse(configured, out var parsed) || parsed < minimum || parsed > maximum)
            {
                throw new InvalidOperationException(
                    $"Configuration '{key}' must be an integer between {minimum} and {maximum}.");
            }

            return parsed;
        }
    }
}
