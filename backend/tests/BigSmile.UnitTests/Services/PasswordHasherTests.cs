using BigSmile.Infrastructure.Services;
using Xunit;

namespace BigSmile.UnitTests.Services
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new PasswordHasher();

        [Fact]
        public void HashPassword_ReturnsDifferentHashEachTime()
        {
            // Arrange
            var password = "MySecurePassword123";

            // Act
            var hash1 = _hasher.HashPassword(password);
            var hash2 = _hasher.HashPassword(password);

            // Assert
            Assert.NotEqual(hash1, hash2); // Different salt each time
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var password = "MySecurePassword123";
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(hash, password);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            // Arrange
            var password = "MySecurePassword123";
            var wrongPassword = "WrongPassword";
            var hash = _hasher.HashPassword(password);

            // Act
            var result = _hasher.VerifyPassword(hash, wrongPassword);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_DifferentHash_ReturnsFalse()
        {
            // Arrange
            var password = "MySecurePassword123";
            var hash = _hasher.HashPassword(password);
            var anotherHash = _hasher.HashPassword("AnotherPassword");

            // Act
            var result = _hasher.VerifyPassword(anotherHash, password);

            // Assert
            Assert.False(result);
        }
    }
}