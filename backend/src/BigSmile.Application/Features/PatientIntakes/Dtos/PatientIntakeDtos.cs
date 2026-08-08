using System.Security.Cryptography;
using System.Text;
using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientIntakes.Dtos
{
    public sealed record PatientIntakeMedicalAnswerDto(
        string QuestionKey,
        string Answer,
        string? Details);

    public sealed record PatientIntakeDto(
        string Origin,
        string Status,
        string? FirstName,
        string? LastName,
        DateOnly? DateOfBirth,
        string Sex,
        string? Occupation,
        string MaritalStatus,
        string? ReferredBy,
        string? PreferredPhone,
        string? MobilePhone,
        string? HomePhone,
        string? WorkPhone,
        string? Email,
        string? ResponsiblePartyName,
        string? ResponsiblePartyRelationship,
        string? ResponsiblePartyPhone,
        string? ReasonForVisit,
        IReadOnlyList<PatientIntakeMedicalAnswerDto> MedicalAnswers,
        int CurrentRevisionNumber,
        string ConcurrencyToken,
        DateTime CreatedAtUtc,
        DateTime LastUpdatedAtUtc,
        DateTime? LastEffectiveSavedAtUtc,
        DateTime? SubmittedAtUtc,
        DateTime ExpiresAtUtc);

    public sealed record SavePatientIntakeMedicalAnswerCommand(
        string QuestionKey,
        ClinicalMedicalAnswerValue Answer,
        string? Details);

    public sealed record SavePatientIntakeDraftCommand(
        string? FirstName,
        string? LastName,
        DateOnly? DateOfBirth,
        PatientSex Sex,
        string? Occupation,
        PatientMaritalStatus MaritalStatus,
        string? ReferredBy,
        string? PreferredPhone,
        string? MobilePhone,
        string? HomePhone,
        string? WorkPhone,
        string? Email,
        string? ResponsiblePartyName,
        string? ResponsiblePartyRelationship,
        string? ResponsiblePartyPhone,
        string? ReasonForVisit,
        IReadOnlyList<SavePatientIntakeMedicalAnswerCommand> MedicalAnswers,
        string ConcurrencyToken)
    {
        public PatientIntakeDraftData ToDraftData()
        {
            if (MedicalAnswers is null)
            {
                throw new ArgumentException(
                    "Patient intake medical answers are required.",
                    nameof(MedicalAnswers));
            }

            return new PatientIntakeDraftData(
                FirstName,
                LastName,
                DateOfBirth,
                Sex,
                Occupation,
                MaritalStatus,
                ReferredBy,
                PreferredPhone,
                MobilePhone,
                HomePhone,
                WorkPhone,
                Email,
                ResponsiblePartyName,
                ResponsiblePartyRelationship,
                ResponsiblePartyPhone,
                ReasonForVisit,
                MedicalAnswers
                    .Select(answer => new PatientIntakeMedicalAnswerData(
                        answer.QuestionKey,
                        answer.Answer,
                        answer.Details))
                    .ToArray());
        }
    }

    public enum PatientIntakeCreateFailure
    {
        None = 0,
        SessionInvalid = 1,
        ActiveDraftExists = 2,
        ConcurrentConflict = 3,
        IntakeAlreadySubmitted = 4
    }

    public sealed record PatientIntakeCreateResult(
        PatientIntakeDto? Intake,
        PatientIntakeCreateFailure Failure)
    {
        public bool Succeeded => Intake is not null && Failure == PatientIntakeCreateFailure.None;

        public static PatientIntakeCreateResult Success(PatientIntakeDto intake)
        {
            ArgumentNullException.ThrowIfNull(intake);
            return new PatientIntakeCreateResult(intake, PatientIntakeCreateFailure.None);
        }

        public static PatientIntakeCreateResult Failed(PatientIntakeCreateFailure failure)
        {
            if (failure == PatientIntakeCreateFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeCreateResult(null, failure);
        }
    }

    public enum PatientIntakeSaveFailure
    {
        None = 0,
        SessionInvalid = 1,
        Missing = 2,
        Expired = 3,
        ConcurrentConflict = 4
    }

    public sealed record PatientIntakeSaveResult(
        PatientIntakeDto? Intake,
        bool Changed,
        PatientIntakeSaveFailure Failure)
    {
        public bool Succeeded => Intake is not null && Failure == PatientIntakeSaveFailure.None;

        public static PatientIntakeSaveResult Success(PatientIntakeDto intake, bool changed)
        {
            ArgumentNullException.ThrowIfNull(intake);
            return new PatientIntakeSaveResult(intake, changed, PatientIntakeSaveFailure.None);
        }

        public static PatientIntakeSaveResult Failed(PatientIntakeSaveFailure failure)
        {
            if (failure == PatientIntakeSaveFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeSaveResult(null, false, failure);
        }
    }

    public sealed record PatientIntakeSaveResponseDto(
        PatientIntakeDto Intake,
        bool Changed);

    public enum PatientIntakeSubmitFailure
    {
        None = 0,
        SessionInvalid = 1,
        Missing = 2,
        Expired = 3,
        Incomplete = 4,
        ConcurrentConflict = 5
    }

    public sealed record PatientIntakeSubmitResult(
        PatientIntakeDto? Intake,
        bool Changed,
        PatientIntakeSubmitFailure Failure)
    {
        public bool Succeeded => Intake is not null && Failure == PatientIntakeSubmitFailure.None;

        public static PatientIntakeSubmitResult Success(PatientIntakeDto intake, bool changed)
        {
            ArgumentNullException.ThrowIfNull(intake);
            return new PatientIntakeSubmitResult(intake, changed, PatientIntakeSubmitFailure.None);
        }

        public static PatientIntakeSubmitResult Failed(PatientIntakeSubmitFailure failure)
        {
            if (failure == PatientIntakeSubmitFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeSubmitResult(null, false, failure);
        }
    }

    public sealed record PatientIntakeSubmitResponseDto(
        PatientIntakeDto Intake,
        bool Changed);

    internal static class PatientIntakeDtoMappings
    {
        public static PatientIntakeDto ToDto(this PatientIntake intake)
        {
            ArgumentNullException.ThrowIfNull(intake);

            var draft = intake.GetDraftData();
            return new PatientIntakeDto(
                intake.Origin.ToString(),
                intake.Status.ToString(),
                draft.FirstName,
                draft.LastName,
                draft.DateOfBirth,
                draft.Sex.ToString(),
                draft.Occupation,
                draft.MaritalStatus.ToString(),
                draft.ReferredBy,
                draft.PreferredPhone,
                draft.MobilePhone,
                draft.HomePhone,
                draft.WorkPhone,
                draft.Email,
                draft.ResponsiblePartyName,
                draft.ResponsiblePartyRelationship,
                draft.ResponsiblePartyPhone,
                draft.ReasonForVisit,
                draft.MedicalAnswers
                    .Select(answer => new PatientIntakeMedicalAnswerDto(
                        answer.QuestionKey,
                        answer.Answer.ToString(),
                        answer.Details))
                    .ToArray(),
                intake.CurrentRevisionNumber,
                PatientIntakeConcurrencyToken.Create(intake),
                intake.CreatedAtUtc,
                intake.LastUpdatedAtUtc,
                intake.LastEffectiveSavedAtUtc,
                intake.SubmittedAtUtc,
                intake.ExpiresAtUtc);
        }
    }

    internal static class PatientIntakeConcurrencyToken
    {
        private const string RowVersionPrefix = "rv1.";
        private const string FallbackPrefix = "fb1.";

        public static string Create(PatientIntake intake)
        {
            ArgumentNullException.ThrowIfNull(intake);

            if (intake.RowVersion.Length > 0)
            {
                return RowVersionPrefix + Convert.ToBase64String(intake.RowVersion);
            }

            var material = string.Join(
                '|',
                intake.Id.ToString("N"),
                intake.CurrentRevisionNumber,
                intake.LastUpdatedAtUtc.Ticks,
                intake.Status.ToString());
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
            return FallbackPrefix + Convert.ToBase64String(hash);
        }

        public static bool Matches(PatientIntake intake, string? suppliedToken)
        {
            if (string.IsNullOrWhiteSpace(suppliedToken))
            {
                return false;
            }

            var expectedBytes = Encoding.UTF8.GetBytes(Create(intake));
            var suppliedBytes = Encoding.UTF8.GetBytes(suppliedToken.Trim());
            return expectedBytes.Length == suppliedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes);
        }
    }
}
