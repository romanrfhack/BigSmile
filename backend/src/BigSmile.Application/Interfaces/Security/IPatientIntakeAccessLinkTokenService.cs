namespace BigSmile.Application.Interfaces.Security
{
    public sealed record GeneratedPatientIntakeAccessLinkToken(
        string RawToken,
        string TokenHash);

    public interface IPatientIntakeAccessLinkTokenService
    {
        GeneratedPatientIntakeAccessLinkToken Generate();
        string ComputeHash(string rawToken);
        bool VerifyHash(string rawToken, string expectedTokenHash);
    }
}
