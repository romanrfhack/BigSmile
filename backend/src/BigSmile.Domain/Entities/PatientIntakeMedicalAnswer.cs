using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientIntakeMedicalAnswer : Entity<Guid>, ITenantOwnedEntity
    {
        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientIntakeId { get; private set; }
        public PatientIntake PatientIntake { get; private set; } = null!;

        public string QuestionKey { get; private set; } = string.Empty;
        public ClinicalMedicalAnswerValue Answer { get; private set; } = ClinicalMedicalAnswerValue.Unknown;
        public string? Details { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }

        private PatientIntakeMedicalAnswer()
        {
        }

        internal PatientIntakeMedicalAnswer(
            PatientIntake patientIntake,
            string questionKey,
            ClinicalMedicalAnswerValue answer,
            string? details,
            DateTime occurredAtUtc)
        {
            ArgumentNullException.ThrowIfNull(patientIntake);
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            Id = Guid.NewGuid();
            TenantId = patientIntake.TenantId;
            PatientIntakeId = patientIntake.Id;
            PatientIntake = patientIntake;
            QuestionKey = ClinicalMedicalQuestionnaireCatalog.NormalizeQuestionKey(questionKey);
            Answer = EnsureDefinedAnswer(answer);
            Details = NormalizeOptional(details, nameof(details), ClinicalMedicalAnswer.DetailsMaxLength);
            LastUpdatedAtUtc = occurredAtUtc;
        }

        internal bool Update(
            ClinicalMedicalAnswerValue answer,
            string? details,
            DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            var normalizedAnswer = EnsureDefinedAnswer(answer);
            var normalizedDetails = NormalizeOptional(
                details,
                nameof(details),
                ClinicalMedicalAnswer.DetailsMaxLength);

            if (Answer == normalizedAnswer &&
                string.Equals(Details, normalizedDetails, StringComparison.Ordinal))
            {
                return false;
            }

            Answer = normalizedAnswer;
            Details = normalizedDetails;
            LastUpdatedAtUtc = occurredAtUtc;
            return true;
        }

        private static ClinicalMedicalAnswerValue EnsureDefinedAnswer(
            ClinicalMedicalAnswerValue answer)
        {
            if (!Enum.IsDefined(typeof(ClinicalMedicalAnswerValue), answer))
            {
                throw new ArgumentException(
                    "Patient intake medical answer is not supported.",
                    nameof(answer));
            }

            return answer;
        }

        private static string? NormalizeOptional(
            string? value,
            string paramName,
            int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException(
                    $"{paramName} exceeds the allowed length of {maxLength}.",
                    paramName);
            }

            return normalized;
        }

        private static void EnsureUtc(DateTime value, string paramName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient intake timestamps must be UTC.",
                    paramName);
            }
        }
    }
}
