using System.Security.Cryptography;
using System.Text;
using BigSmile.Application.Interfaces.Security;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientIntakeAccessLinkTokenService
        : IPatientIntakeAccessLinkTokenService
    {
        private const int TokenSizeBytes = 32;
        private const int TokenHashSizeBytes = 32;

        public GeneratedPatientIntakeAccessLinkToken Generate()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);
            var rawToken = ToBase64Url(tokenBytes);
            return new GeneratedPatientIntakeAccessLinkToken(
                rawToken,
                ComputeHash(rawToken));
        }

        public string ComputeHash(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                throw new ArgumentException(
                    "Patient intake access link token is required.",
                    nameof(rawToken));
            }

            return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(rawToken.Trim())));
        }

        public bool VerifyHash(string rawToken, string expectedTokenHash)
        {
            if (string.IsNullOrWhiteSpace(rawToken) ||
                string.IsNullOrWhiteSpace(expectedTokenHash))
            {
                return false;
            }

            try
            {
                var expectedBytes = Convert.FromHexString(expectedTokenHash.Trim());
                if (expectedBytes.Length != TokenHashSizeBytes)
                {
                    return false;
                }

                var actualBytes = SHA256.HashData(
                    Encoding.UTF8.GetBytes(rawToken.Trim()));
                return CryptographicOperations.FixedTimeEquals(
                    actualBytes,
                    expectedBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
