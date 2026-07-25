using System.Text.Json;
using System.Text.Json.Serialization;

namespace BigSmile.Domain.Entities
{
    public sealed record PatientIntakeMedicalAnswerData(
        string QuestionKey,
        ClinicalMedicalAnswerValue Answer,
        string? Details);

    public sealed record PatientIntakeDraftData(
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
        IReadOnlyList<PatientIntakeMedicalAnswerData> MedicalAnswers)
    {
        public static PatientIntakeDraftData Empty()
        {
            return new PatientIntakeDraftData(
                null,
                null,
                null,
                PatientSex.Unspecified,
                null,
                PatientMaritalStatus.Unspecified,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys
                    .Select(questionKey => new PatientIntakeMedicalAnswerData(
                        questionKey,
                        ClinicalMedicalAnswerValue.Unknown,
                        null))
                    .ToArray());
        }
    }

    internal static class PatientIntakeSnapshotSerializer
    {
        public const int CurrentSchemaVersion = 1;

        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };

        public static string SerializeSnapshot(PatientIntakeDraftData snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            return JsonSerializer.Serialize(
                new PatientIntakeSnapshotEnvelope(CurrentSchemaVersion, snapshot),
                Options);
        }

        public static string SerializeChangedFields(IEnumerable<string> changedFields)
        {
            ArgumentNullException.ThrowIfNull(changedFields);

            var normalized = changedFields
                .Where(field => !string.IsNullOrWhiteSpace(field))
                .Select(field => field.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(field => field, StringComparer.Ordinal)
                .ToArray();

            if (normalized.Length == 0)
            {
                throw new ArgumentException(
                    "At least one changed patient intake field is required.",
                    nameof(changedFields));
            }

            return JsonSerializer.Serialize(normalized, Options);
        }

        private sealed record PatientIntakeSnapshotEnvelope(
            int SchemaVersion,
            PatientIntakeDraftData Data);
    }
}
