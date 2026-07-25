namespace BigSmile.Application.Interfaces.Security
{
    public sealed class GeneratedPatientIntakeAccessLinkToken
    {
        public GeneratedPatientIntakeAccessLinkToken(string rawToken, string tokenHash)
        {
            RawToken = string.IsNullOrWhiteSpace(rawToken)
                ? throw new ArgumentException(
                    "Generated patient intake access link token is required.",
                    nameof(rawToken))
                : rawToken;
            TokenHash = string.IsNullOrWhiteSpace(tokenHash)
                ? throw new ArgumentException(
                    "Generated patient intake access link token hash is required.",
                    nameof(tokenHash))
                : tokenHash;
        }

        public string RawToken { get; }
        public string TokenHash { get; }
    }

    public interface IPatientIntakeAccessLinkTokenService
    {
        GeneratedPatientIntakeAccessLinkToken Generate();
        string ComputeHash(string rawToken);
        bool VerifyHash(string rawToken, string expectedTokenHash);
    }
}
