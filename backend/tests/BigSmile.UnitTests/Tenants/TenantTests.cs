using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Tenants
{
    public sealed class TenantTests
    {
        [Fact]
        public void Constructor_UsesDocumentedDefaultTimeZone()
        {
            var tenant = new Tenant("Dental Clinic", "dental-clinic");

            Assert.Equal(Tenant.DefaultTimeZoneId, tenant.TimeZoneId);
            Assert.Equal(Tenant.DefaultTimeZoneId, TimeZoneInfo.FindSystemTimeZoneById(tenant.TimeZoneId).Id);
        }

        [Fact]
        public void Constructor_AcceptsAvailableTimeZone()
        {
            var tenant = new Tenant("Dental Clinic", "dental-clinic", "UTC");

            Assert.Equal("UTC", tenant.TimeZoneId);
        }

        [Fact]
        public void Constructor_RejectsUnavailableTimeZone()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                new Tenant("Dental Clinic", "dental-clinic", "Invalid/BigSmile-Time-Zone"));

            Assert.Contains("not available", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UpdateTimeZone_ChangesValueAndUpdateMetadata()
        {
            var tenant = new Tenant("Dental Clinic", "dental-clinic");

            tenant.UpdateTimeZone("UTC");

            Assert.Equal("UTC", tenant.TimeZoneId);
            Assert.NotNull(tenant.UpdatedAt);
        }
    }
}
