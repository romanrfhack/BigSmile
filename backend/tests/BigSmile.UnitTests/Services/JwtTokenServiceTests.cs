using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace BigSmile.UnitTests.Services
{
    public class JwtTokenServiceTests
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtTokenService _service;

        public JwtTokenServiceTests()
        {
            _jwtSettings = new JwtSettings
            {
                Secret = "SuperSecretKeyThatIsAtLeast32CharactersLong!",
                Issuer = "BigSmile",
                Audience = "BigSmile",
                ExpirationMinutes = 120
            };
            _service = new JwtTokenService(_jwtSettings);
        }

        [Fact]
        public void GenerateToken_WithTenant_IncludesTenantClaims()
        {
            // Arrange
            var user = new User("test@example.com", "hashedPassword", "Test User");
            var tenant = new Tenant("Test Tenant", "test-tenant");
            var role = new Role("TenantAdmin", "Tenant administrator");

            // Act
            var token = _service.GenerateToken(user, tenant, role);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal(_jwtSettings.Issuer, jwt.Issuer);
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
            Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == role.Name);
            Assert.Contains(jwt.Claims, c => c.Type == "tenant_id" && c.Value == tenant.Id.ToString());
            Assert.Contains(jwt.Claims, c => c.Type == "tenant_name" && c.Value == tenant.Name);
        }

        [Fact]
        public void GenerateToken_WithoutTenant_ExcludesTenantClaims()
        {
            // Arrange
            var user = new User("test@example.com", "hashedPassword", "Test User");
            var role = new Role("PlatformAdmin", "Platform administrator");

            // Act
            var token = _service.GenerateToken(user, null, role);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.DoesNotContain(jwt.Claims, c => c.Type == "tenant_id");
            Assert.DoesNotContain(jwt.Claims, c => c.Type == "tenant_name");
            Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == role.Name);
        }

        [Fact]
        public void GenerateToken_Expiration_ShouldBeSet()
        {
            // Arrange
            var user = new User("test@example.com", "hashedPassword", "Test User");
            var tenant = new Tenant("Test Tenant", "test-tenant");
            var role = new Role("TenantUser", "User");

            // Act
            var token = _service.GenerateToken(user, tenant, role);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            // Should be approximately _jwtSettings.ExpirationMinutes from now
            var expected = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);
            var tolerance = TimeSpan.FromSeconds(5);
            Assert.InRange(jwt.ValidTo, expected - tolerance, expected + tolerance);
        }
    }
}