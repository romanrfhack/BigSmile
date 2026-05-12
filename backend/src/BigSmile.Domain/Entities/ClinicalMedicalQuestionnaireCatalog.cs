namespace BigSmile.Domain.Entities
{
    public static class ClinicalMedicalQuestionnaireCatalog
    {
        public const int QuestionKeyMaxLength = 80;

        private static readonly string[] QuestionKeys =
        {
            "currentMedicalTreatment",
            "regularMedication",
            "priorSurgery",
            "bloodTransfusion",
            "drugUse",
            "allergicReactions",
            "allergyPenicillin",
            "allergyAnesthetics",
            "allergyAspirin",
            "allergySulfas",
            "allergyIodine",
            "allergyOther",
            "hypertension",
            "hypotension",
            "excessiveBleeding",
            "bloodOrCoagulationDisorder",
            "anemiaHemophiliaVitaminKDeficiency",
            "retroviralTreatment",
            "badDentalExperience",
            "covidHistory",
            "sexuallyTransmittedDisease",
            "congenitalOrCurrentHeartDisease",
            "hepatitis",
            "endocarditis",
            "seizures",
            "diabetes",
            "tuberculosis",
            "hyperthyroidism",
            "hypothyroidism",
            "heartAttackOrAngina",
            "openHeartSurgery",
            "recurrentHerpesOrAphthae",
            "bitesNailsOrLips",
            "smokes",
            "acidicFoodConsumption",
            "bruxismAtNight",
            "pregnantLactatingOrSuspected",
            "contraceptiveMedication",
            "anesthesiaComplications"
        };

        private static readonly HashSet<string> QuestionKeySet = new(QuestionKeys, StringComparer.Ordinal);

        public static IReadOnlyList<string> AllowedQuestionKeys { get; } = Array.AsReadOnly(QuestionKeys);

        public static bool IsAllowedQuestionKey(string? questionKey)
        {
            if (string.IsNullOrWhiteSpace(questionKey))
            {
                return false;
            }

            return QuestionKeySet.Contains(questionKey.Trim());
        }

        public static string NormalizeQuestionKey(string? questionKey)
        {
            if (string.IsNullOrWhiteSpace(questionKey))
            {
                throw new ArgumentException("Medical questionnaire question key is required.", nameof(questionKey));
            }

            var normalized = questionKey.Trim();
            if (normalized.Length > QuestionKeyMaxLength)
            {
                throw new ArgumentException(
                    $"Medical questionnaire question key exceeds the allowed length of {QuestionKeyMaxLength}.",
                    nameof(questionKey));
            }

            if (!QuestionKeySet.Contains(normalized))
            {
                throw new ArgumentException("Medical questionnaire question key is not supported.", nameof(questionKey));
            }

            return normalized;
        }
    }
}
