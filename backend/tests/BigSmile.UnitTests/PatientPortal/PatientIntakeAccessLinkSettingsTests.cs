using BigSmile.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAccessLinkSettingsTests
    {
        [Fact]
        public void Constructor_UsesThirtyMinuteDefault()
        {
            var settings = new PatientIntakeAccessLinkSettings(
                new ConfigurationBuilder().Build());

            Assert.Equal(TimeSpan.FromMinutes(30), settings.WaitingRoomLinkLifetime);
        }

        [Fact]
        public void Constructor_UsesConfiguredBoundedLifetime()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientIntakeAccessLinkSettings.WaitingRoomLinkLifetimeMinutesKey] = "45"
                })
                .Build();

            var settings = new PatientIntakeAccessLinkSettings(configuration);

            Assert.Equal(TimeSpan.FromMinutes(45), settings.WaitingRoomLinkLifetime);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("4")]
        [InlineData("121")]
        [InlineData("not-a-number")]
        public void Constructor_RejectsOutOfRangeOrInvalidLifetime(string value)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientIntakeAccessLinkSettings.WaitingRoomLinkLifetimeMinutesKey] = value
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                new PatientIntakeAccessLinkSettings(configuration));
        }
    }
}
