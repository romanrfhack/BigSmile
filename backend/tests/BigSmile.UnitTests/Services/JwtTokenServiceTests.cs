using BigSmile.Application.Authorization;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            var role = new Role(SystemRoles.TenantAdmin, "Tenant administrator");
            var descriptor = new AuthTokenDescriptor(
                role.Name,
                AccessScope.Tenant,
                new[] { Permissions.AuthSelfRead, Permissions.TenantRead },
                tenant);

            // Act
            var token = _service.GenerateToken(user, descriptor);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.Equal(_jwtSettings.Issuer, jwt.Issuer);
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
            Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == role.Name);
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.Role && c.Value == role.Name);
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.Scope && c.Value == AccessScope.Tenant.ToClaimValue());
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.Permission && c.Value == Permissions.TenantRead);
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.TenantId && c.Value == tenant.Id.ToString());
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.TenantName && c.Value == tenant.Name);
        }

        [Fact]
        public void GenerateToken_WithoutTenant_ExcludesTenantClaims()
        {
            // Arrange
            var user = new User("test@example.com", "hashedPassword", "Test User");
            var role = new Role(SystemRoles.PlatformAdmin, "Platform administrator");
            var descriptor = new AuthTokenDescriptor(
                role.Name,
                AccessScope.Platform,
                new[] { Permissions.PlatformTenantsRead });

            // Act
            var token = _service.GenerateToken(user, descriptor);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            // Assert
            Assert.DoesNotContain(jwt.Claims, c => c.Type == BigSmileClaimTypes.TenantId);
            Assert.DoesNotContain(jwt.Claims, c => c.Type == BigSmileClaimTypes.TenantName);
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.Scope && c.Value == AccessScope.Platform.ToClaimValue());
            Assert.Contains(jwt.Claims, c => c.Type == BigSmileClaimTypes.Permission && c.Value == Permissions.PlatformTenantsRead);
        }

        [Fact]
        public void GenerateToken_Expiration_ShouldBeSet()
        {
            // Arrange
            var user = new User("test@example.com", "hashedPassword", "Test User");
            var tenant = new Tenant("Test Tenant", "test-tenant");
            var role = new Role(SystemRoles.TenantUser, "User");
            var descriptor = new AuthTokenDescriptor(
                role.Name,
                AccessScope.Branch,
                new[] { Permissions.AuthSelfRead, Permissions.BranchReadAssigned },
                tenant,
                tenant.AddBranch("Main"));

            // Act
            var token = _service.GenerateToken(user, descriptor);
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
