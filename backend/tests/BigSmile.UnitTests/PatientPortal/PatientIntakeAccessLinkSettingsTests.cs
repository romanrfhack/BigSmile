using BigSmile.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAccessLinkSettingsTests
    {
        [Fact]
        public void Constructor_UsesApprovedThirtyMinuteDefault()
        {
            var settings = new PatientIntakeAccessLinkSettings(
                new ConfigurationBuilder().Build());

            Assert.Equal(TimeSpan.FromMinutes(30), settings.WaitingRoomLinkLifetime);
        }

        [Theory]
        [InlineData("5", 5)]
        [InlineData("30", 30)]
        [InlineData("120", 120)]
        public void Constructor_AcceptsBoundedConfiguredMinutes(
            string configuredMinutes,
            int expectedMinutes)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientIntakeAccessLinkSettings.LifetimeMinutesKey] = configuredMinutes
                })
                .Build();

            var settings = new PatientIntakeAccessLinkSettings(configuration);

            Assert.Equal(
                TimeSpan.FromMinutes(expectedMinutes),
                settings.WaitingRoomLinkLifetime);
        }

        [Theory]
        [InlineData("4")]
        [InlineData("121")]
        [InlineData("not-a-number")]
        public void Constructor_RejectsInvalidOrOutOfRangeConfiguration(
            string configuredMinutes)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientIntakeAccessLinkSettings.LifetimeMinutesKey] = configuredMinutes
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                new PatientIntakeAccessLinkSettings(configuration));
        }
    }
}
