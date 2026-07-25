using BigSmile.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeDraftSettingsTests
    {
        [Fact]
        public void Constructor_UsesApprovedThirtyDayDefault()
        {
            var settings = new PatientIntakeDraftSettings(
                new ConfigurationBuilder().Build());

            Assert.Equal(TimeSpan.FromDays(30), settings.DraftLifetime);
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("45", 45)]
        [InlineData("365", 365)]
        public void Constructor_AcceptsBoundedConfiguredDays(
            string configuredDays,
            int expectedDays)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientIntakeDraftSettings.DraftLifetimeDaysKey] = configuredDays
                })
                .Build();

            var settings = new PatientIntakeDraftSettings(configuration);

            Assert.Equal(TimeSpan.FromDays(expectedDays), settings.DraftLifetime);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("366")]
        [InlineData("not-a-number")]
        public void Constructor_RejectsInvalidOrOutOfRangeConfiguration(
            string configuredDays)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [PatientIntakeDraftSettings.DraftLifetimeDaysKey] = configuredDays
                })
                .Build();

            Assert.Throws<InvalidOperationException>(() =>
                new PatientIntakeDraftSettings(configuration));
        }
    }
}
