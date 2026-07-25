using BigSmile.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalInvitationSettingsTests
    {
        [Fact]
        public void Constructor_UsesApprovedTwentyFourHourDefault()
        {
            var settings = new PatientPortalInvitationSettings(new ConfigurationBuilder().Build());

            Assert.Equal(TimeSpan.FromHours(24), settings.ExistingPatientActivationLifetime);
        }

        [Fact]
        public void Constructor_UsesConfiguredLifetime()
        {
            var values = new Dictionary<string, string?>
            {
                [PatientPortalInvitationSettings.ConfigurationKey] = "48"
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

            var settings = new PatientPortalInvitationSettings(configuration);

            Assert.Equal(TimeSpan.FromHours(48), settings.ExistingPatientActivationLifetime);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("169")]
        [InlineData("invalid")]
        public void Constructor_RejectsUnsafeConfiguration(string value)
        {
            var values = new Dictionary<string, string?>
            {
                [PatientPortalInvitationSettings.ConfigurationKey] = value
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(values)
                .Build();

            Assert.Throws<InvalidOperationException>(() => new PatientPortalInvitationSettings(configuration));
        }
    }
}
