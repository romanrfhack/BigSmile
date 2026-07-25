using BigSmile.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalAuthenticationSettingsTests
    {
        [Fact]
        public void Constructor_UsesApprovedSecurityDefaults()
        {
            var settings = new PatientPortalAuthenticationSettings(
                new ConfigurationBuilder().Build());

            Assert.Equal(100_000, settings.PasswordHashIterationCount);
            Assert.Equal(12, settings.MinimumPasswordLength);
            Assert.Equal(128, settings.MaximumPasswordLength);
            Assert.Equal(5, settings.MaximumFailedLoginAttempts);
            Assert.Equal(TimeSpan.FromMinutes(15), settings.LockoutDuration);
        }

        [Theory]
        [InlineData(PatientPortalAuthenticationSettings.PasswordHashIterationCountKey, "99999")]
        [InlineData(PatientPortalAuthenticationSettings.MinimumPasswordLengthKey, "11")]
        [InlineData(PatientPortalAuthenticationSettings.MaximumFailedLoginAttemptsKey, "0")]
        [InlineData(PatientPortalAuthenticationSettings.LockoutDurationMinutesKey, "0")]
        public void Constructor_RejectsConfigurationBelowSecurityBounds(string key, string value)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [key] = value
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                new PatientPortalAuthenticationSettings(configuration));
        }

        [Fact]
        public void Constructor_RejectsMaximumPasswordLengthBelowConfiguredMinimum()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientPortalAuthenticationSettings.MinimumPasswordLengthKey] = "20",
                    [PatientPortalAuthenticationSettings.MaximumPasswordLengthKey] = "19"
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                new PatientPortalAuthenticationSettings(configuration));
        }
    }
}
