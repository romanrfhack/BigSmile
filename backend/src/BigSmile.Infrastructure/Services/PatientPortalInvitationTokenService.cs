using System.Security.Cryptography;
using System.Text;
using BigSmile.Application.Interfaces.Security;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientPortalInvitationTokenService : IPatientPortalInvitationTokenService
    {
        private const int TokenSizeBytes = 32;

        public GeneratedPatientPortalInvitationToken Generate()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);
            var rawToken = ToBase64Url(tokenBytes);
            return new GeneratedPatientPortalInvitationToken(rawToken, ComputeHash(rawToken));
        }

        public string ComputeHash(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken))
            {
                throw new ArgumentException("Patient portal invitation token is required.", nameof(rawToken));
            }

            var tokenBytes = Encoding.UTF8.GetBytes(rawToken.Trim());
            return Convert.ToHexString(SHA256.HashData(tokenBytes));
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
