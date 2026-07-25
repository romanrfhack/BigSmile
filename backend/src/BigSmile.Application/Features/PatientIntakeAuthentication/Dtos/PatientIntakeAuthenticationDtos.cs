namespace BigSmile.Application.Features.PatientIntakeAuthentication.Dtos
{
    public sealed record ActivatePatientIntakeAccountCommand(
        string AccessToken,
        string LoginName,
        string Password);

    public sealed record LoginPatientIntakeAccountCommand(
        string TenantSubdomain,
        string LoginName,
        string Password);

    public sealed record CurrentPatientIntakeSessionDto(
        Guid AccountId,
        Guid IntakeId,
        string TenantSubdomain,
        string LoginName,
        int SessionVersion);

    public sealed record PatientIntakeAuthenticationResponseDto(
        string AccessToken,
        DateTime ExpiresAtUtc,
        CurrentPatientIntakeSessionDto Current);

    public enum PatientIntakeActivationFailure
    {
        None = 0,
        InvalidActivation = 1,
        LoginNameUnavailable = 2,
        ConcurrentConflict = 3
    }

    public sealed record PatientIntakeActivationResult(
        PatientIntakeAuthenticationResponseDto? Authentication,
        PatientIntakeActivationFailure Failure)
    {
        public bool Succeeded =>
            Authentication is not null &&
            Failure == PatientIntakeActivationFailure.None;

        public static PatientIntakeActivationResult Success(
            PatientIntakeAuthenticationResponseDto authentication)
        {
            ArgumentNullException.ThrowIfNull(authentication);
            return new PatientIntakeActivationResult(
                authentication,
                PatientIntakeActivationFailure.None);
        }

        public static PatientIntakeActivationResult Failed(
            PatientIntakeActivationFailure failure)
        {
            if (failure == PatientIntakeActivationFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeActivationResult(null, failure);
        }
    }
}
