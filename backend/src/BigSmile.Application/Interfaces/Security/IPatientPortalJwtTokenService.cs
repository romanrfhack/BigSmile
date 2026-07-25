using BigSmile.Domain.Entities;

namespace BigSmile.Application.Interfaces.Security
{
    public sealed record GeneratedPatientPortalAccessToken(
        string Token,
        DateTime ExpiresAtUtc);

    public interface IPatientPortalJwtTokenService
    {
        GeneratedPatientPortalAccessToken Generate(
            PatientPortalAccount account,
            DateTime issuedAtUtc);

        GeneratedPatientPortalAccessToken GenerateForIntake(
            PatientPortalAccount account,
            PatientIntake intake,
            DateTime issuedAtUtc);
    }

    public interface IPatientPortalJwtSettings
    {
        string Secret { get; }
        string Issuer { get; }
        string Audience { get; }
        TimeSpan AccessTokenLifetime { get; }
    }
}
