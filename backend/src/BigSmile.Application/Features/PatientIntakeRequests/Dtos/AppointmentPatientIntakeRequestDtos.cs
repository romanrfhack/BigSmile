namespace BigSmile.Application.Features.PatientIntakeRequests.Dtos
{
    public sealed record AppointmentPatientIntakeRequestStatusDto(
        Guid AppointmentId,
        Guid PatientId,
        string PatientFullName,
        string? PatientPrimaryPhone,
        string PatientPortalRealm,
        string PortalAccessStatus,
        string IntakeStatus,
        string RecommendedAccess,
        bool CanRequest,
        DateTime? SubmittedAtUtc);

    public sealed record PreparedAppointmentPatientIntakeRequestDto(
        AppointmentPatientIntakeRequestStatusDto Status,
        string AccessMode,
        string? ActivationToken);

    public enum AppointmentPatientIntakeRequestFailure
    {
        None = 0,
        NotFound = 1,
        AlreadyCompleted = 2,
        RecoveryRequired = 3,
        Unavailable = 4
    }

    public sealed record AppointmentPatientIntakeRequestResult(
        PreparedAppointmentPatientIntakeRequestDto? PreparedRequest,
        AppointmentPatientIntakeRequestFailure Failure)
    {
        public bool Succeeded =>
            PreparedRequest is not null &&
            Failure == AppointmentPatientIntakeRequestFailure.None;

        public static AppointmentPatientIntakeRequestResult Success(
            PreparedAppointmentPatientIntakeRequestDto preparedRequest)
        {
            ArgumentNullException.ThrowIfNull(preparedRequest);
            return new AppointmentPatientIntakeRequestResult(
                preparedRequest,
                AppointmentPatientIntakeRequestFailure.None);
        }

        public static AppointmentPatientIntakeRequestResult Failed(
            AppointmentPatientIntakeRequestFailure failure)
        {
            if (failure == AppointmentPatientIntakeRequestFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new AppointmentPatientIntakeRequestResult(null, failure);
        }
    }
}
