using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class ClinicalMedicalAnswer : Entity<Guid>, ITenantOwnedEntity
    {
        public const int DetailsMaxLength = 500;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid ClinicalRecordId { get; private set; }
        public ClinicalRecord ClinicalRecord { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public string QuestionKey { get; private set; } = string.Empty;
        public ClinicalMedicalAnswerValue Answer { get; private set; } = ClinicalMedicalAnswerValue.Unknown;
        public string? Details { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }
        public Guid UpdatedByUserId { get; private set; }

        private ClinicalMedicalAnswer()
        {
        }

        internal ClinicalMedicalAnswer(
            Guid tenantId,
            Guid clinicalRecordId,
            Guid patientId,
            string questionKey,
            ClinicalMedicalAnswerValue answer,
            string? details,
            Guid updatedByUserId)
        {
            EnsureTenantId(tenantId);
            EnsureReference(clinicalRecordId, "Clinical record reference is required.", nameof(clinicalRecordId));
            EnsureReference(patientId, "Medical questionnaire patient reference is required.", nameof(patientId));
            EnsureActor(updatedByUserId);

            Id = Guid.NewGuid();
            TenantId = tenantId;
            ClinicalRecordId = clinicalRecordId;
            PatientId = patientId;
            QuestionKey = ClinicalMedicalQuestionnaireCatalog.NormalizeQuestionKey(questionKey);
            Answer = EnsureDefinedAnswer(answer);
            Details = NormalizeOptional(details, nameof(details), DetailsMaxLength);
            UpdatedAtUtc = DateTime.UtcNow;
            UpdatedByUserId = updatedByUserId;
        }

        internal bool Update(ClinicalMedicalAnswerValue answer, string? details, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId);

            var normalizedAnswer = EnsureDefinedAnswer(answer);
            var normalizedDetails = NormalizeOptional(details, nameof(details), DetailsMaxLength);

            if (Answer == normalizedAnswer && string.Equals(Details, normalizedDetails, StringComparison.Ordinal))
            {
                return false;
            }

            Answer = normalizedAnswer;
            Details = normalizedDetails;
            UpdatedAtUtc = DateTime.UtcNow;
            UpdatedByUserId = updatedByUserId;

            return true;
        }

        private static void EnsureTenantId(Guid tenantId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Medical questionnaire tenant ownership is required.", nameof(tenantId));
            }
        }

        private static void EnsureReference(Guid value, string message, string paramName)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        private static void EnsureActor(Guid updatedByUserId)
        {
            if (updatedByUserId == Guid.Empty)
            {
                throw new ArgumentException("Medical questionnaire actor is required.", nameof(updatedByUserId));
            }
        }

        private static ClinicalMedicalAnswerValue EnsureDefinedAnswer(ClinicalMedicalAnswerValue answer)
        {
            if (!Enum.IsDefined(typeof(ClinicalMedicalAnswerValue), answer))
            {
                throw new ArgumentException("Medical questionnaire answer is not supported.", nameof(answer));
            }

            return answer;
        }

        private static string? NormalizeOptional(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} exceeds the allowed length of {maxLength}.", paramName);
            }

            return normalized;
        }
    }
}
