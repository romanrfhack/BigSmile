using BigSmile.Infrastructure.Services;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalInvitationTokenServiceTests
    {
        private readonly PatientPortalInvitationTokenService _service = new();

        [Fact]
        public void Generate_ReturnsBase64UrlTokenAndSha256Hash()
        {
            var generated = _service.Generate();

            Assert.Equal(43, generated.RawToken.Length);
            Assert.Equal(64, generated.TokenHash.Length);
            Assert.DoesNotContain("=", generated.RawToken, StringComparison.Ordinal);
            Assert.DoesNotContain("+", generated.RawToken, StringComparison.Ordinal);
            Assert.DoesNotContain("/", generated.RawToken, StringComparison.Ordinal);
            Assert.Equal(generated.TokenHash, _service.ComputeHash(generated.RawToken));
            Assert.True(_service.VerifyHash(generated.RawToken, generated.TokenHash));
            Assert.NotEqual(generated.RawToken, generated.TokenHash);
        }

        [Fact]
        public void Generate_ReturnsDifferentTokensAcrossInvocations()
        {
            var first = _service.Generate();
            var second = _service.Generate();

            Assert.NotEqual(first.RawToken, second.RawToken);
            Assert.NotEqual(first.TokenHash, second.TokenHash);
            Assert.False(_service.VerifyHash(first.RawToken, second.TokenHash));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-hex")]
        [InlineData("AA")]
        public void VerifyHash_RejectsMalformedExpectedHash(string expectedHash)
        {
            Assert.False(_service.VerifyHash("raw-token", expectedHash));
        }

        [Fact]
        public void ComputeHash_RejectsBlankToken()
        {
            Assert.Throws<ArgumentException>(() => _service.ComputeHash("  "));
        }
    }
}
