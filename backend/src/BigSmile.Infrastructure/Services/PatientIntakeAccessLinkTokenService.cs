using BigSmile.Application.Interfaces.Security;

namespace BigSmile.Infrastructure.Services
{
    public sealed class PatientIntakeAccessLinkTokenService : IPatientIntakeAccessLinkTokenService
    {
        private readonly IPatientPortalInvitationTokenService _innerTokenService;

        public PatientIntakeAccessLinkTokenService(
            IPatientPortalInvitationTokenService innerTokenService)
        {
            _innerTokenService = innerTokenService ?? throw new ArgumentNullException(nameof(innerTokenService));
        }

        public GeneratedPatientIntakeAccessLinkToken Generate()
        {
            var generated = _innerTokenService.Generate();
            return new GeneratedPatientIntakeAccessLinkToken(
                generated.RawToken,
                generated.TokenHash);
        }

        public string ComputeHash(string rawToken)
        {
            return _innerTokenService.ComputeHash(rawToken);
        }

        public bool VerifyHash(string rawToken, string expectedTokenHash)
        {
            return _innerTokenService.VerifyHash(rawToken, expectedTokenHash);
        }
    }
}
