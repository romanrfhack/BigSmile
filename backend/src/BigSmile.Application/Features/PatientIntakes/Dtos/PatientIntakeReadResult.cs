namespace BigSmile.Application.Features.PatientIntakes.Dtos
{
    public enum PatientIntakeReadFailure
    {
        None = 0,
        SessionInvalid = 1,
        Missing = 2
    }

    public sealed record PatientIntakeReadResult(
        PatientIntakeDto? Intake,
        PatientIntakeReadFailure Failure)
    {
        public bool Succeeded => Intake is not null && Failure == PatientIntakeReadFailure.None;

        public static PatientIntakeReadResult Success(PatientIntakeDto intake)
        {
            ArgumentNullException.ThrowIfNull(intake);
            return new PatientIntakeReadResult(intake, PatientIntakeReadFailure.None);
        }

        public static PatientIntakeReadResult Failed(PatientIntakeReadFailure failure)
        {
            if (failure == PatientIntakeReadFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeReadResult(null, failure);
        }
    }
}
