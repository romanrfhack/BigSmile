namespace BigSmile.Application.Interfaces.Security
{
    public sealed class GeneratedPatientPortalInvitationToken
    {
        public GeneratedPatientPortalInvitationToken(string rawToken, string tokenHash)
        {
            RawToken = string.IsNullOrWhiteSpace(rawToken)
                ? throw new ArgumentException("Generated patient portal invitation token is required.", nameof(rawToken))
                : rawToken;
            TokenHash = string.IsNullOrWhiteSpace(tokenHash)
                ? throw new ArgumentException("Generated patient portal invitation token hash is required.", nameof(tokenHash))
                : tokenHash;
        }

        public string RawToken { get; }
        public string TokenHash { get; }
    }

    public interface IPatientPortalInvitationTokenService
    {
        GeneratedPatientPortalInvitationToken Generate();
        string ComputeHash(string rawToken);
        bool VerifyHash(string rawToken, string expectedTokenHash);
    }
}
