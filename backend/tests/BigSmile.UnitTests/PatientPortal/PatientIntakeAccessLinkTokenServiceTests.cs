using BigSmile.Infrastructure.Services;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAccessLinkTokenServiceTests
    {
        [Fact]
        public void Generate_ReturnsDistinctUrlSafeTokensAndHashOnlyVerifier()
        {
            var service = new PatientIntakeAccessLinkTokenService();

            var first = service.Generate();
            var second = service.Generate();

            Assert.NotEqual(first.RawToken, second.RawToken);
            Assert.NotEqual(first.TokenHash, second.TokenHash);
            Assert.Equal(43, first.RawToken.Length);
            Assert.Equal(64, first.TokenHash.Length);
            Assert.DoesNotContain("+", first.RawToken, StringComparison.Ordinal);
            Assert.DoesNotContain("/", first.RawToken, StringComparison.Ordinal);
            Assert.DoesNotContain("=", first.RawToken, StringComparison.Ordinal);
            Assert.True(service.VerifyHash(first.RawToken, first.TokenHash));
            Assert.False(service.VerifyHash(second.RawToken, first.TokenHash));
            Assert.False(service.VerifyHash(first.RawToken, "not-a-hex-hash"));
        }

        [Fact]
        public void ComputeHash_DoesNotReturnRawToken()
        {
            var service = new PatientIntakeAccessLinkTokenService();
            const string rawToken = "a-secure-one-time-token";

            var hash = service.ComputeHash(rawToken);

            Assert.NotEqual(rawToken, hash);
            Assert.True(service.VerifyHash(rawToken, hash));
        }
    }
}
