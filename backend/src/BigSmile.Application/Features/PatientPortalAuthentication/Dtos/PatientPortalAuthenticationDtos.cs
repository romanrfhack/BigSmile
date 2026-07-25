namespace BigSmile.Application.Features.PatientPortalAuthentication.Dtos
{
    public sealed record ActivatePatientPortalAccountCommand(
        string ActivationToken,
        string LoginName,
        string Password);

    public sealed record LoginPatientPortalAccountCommand(
        string TenantSubdomain,
        string LoginName,
        string Password);

    public sealed record CurrentPatientPortalSessionDto(
        Guid AccountId,
        Guid PatientId,
        string TenantSubdomain,
        string LoginName,
        int SessionVersion);

    public sealed record PatientPortalAuthenticationResponseDto(
        string AccessToken,
        DateTime ExpiresAtUtc,
        CurrentPatientPortalSessionDto Current);

    public enum PatientPortalActivationFailure
    {
        None = 0,
        InvalidActivation = 1,
        LoginNameUnavailable = 2,
        ConcurrentConflict = 3
    }

    public sealed record PatientPortalActivationResult(
        PatientPortalAuthenticationResponseDto? Authentication,
        PatientPortalActivationFailure Failure)
    {
        public bool Succeeded => Authentication is not null && Failure == PatientPortalActivationFailure.None;

        public static PatientPortalActivationResult Success(PatientPortalAuthenticationResponseDto authentication)
        {
            ArgumentNullException.ThrowIfNull(authentication);
            return new PatientPortalActivationResult(authentication, PatientPortalActivationFailure.None);
        }

        public static PatientPortalActivationResult Failed(PatientPortalActivationFailure failure)
        {
            if (failure == PatientPortalActivationFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientPortalActivationResult(null, failure);
        }
    }
}
