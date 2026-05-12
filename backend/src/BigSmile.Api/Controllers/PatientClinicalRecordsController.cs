using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using BigSmile.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/clinical-record")]
    public class PatientClinicalRecordsController : ControllerBase
    {
        private readonly IClinicalRecordCommandService _clinicalRecordCommandService;
        private readonly IClinicalRecordQueryService _clinicalRecordQueryService;

        public PatientClinicalRecordsController(
            IClinicalRecordCommandService clinicalRecordCommandService,
            IClinicalRecordQueryService clinicalRecordQueryService)
        {
            _clinicalRecordCommandService = clinicalRecordCommandService ?? throw new ArgumentNullException(nameof(clinicalRecordCommandService));
            _clinicalRecordQueryService = clinicalRecordQueryService ?? throw new ArgumentNullException(nameof(clinicalRecordQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.ClinicalRead)]
        public async Task<ActionResult<ClinicalRecordDetailDto>> GetByPatientId(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicalRecord = await _clinicalRecordQueryService.GetByPatientIdAsync(patientId, cancellationToken);
                if (clinicalRecord is null)
                {
                    return NotFound();
                }

                return Ok(clinicalRecord);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpGet("questionnaire")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalRead)]
        public async Task<ActionResult<ClinicalMedicalQuestionnaireDto>> GetQuestionnaire(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var questionnaire = await _clinicalRecordQueryService.GetQuestionnaireByPatientIdAsync(patientId, cancellationToken);
                if (questionnaire is null)
                {
                    return NotFound();
                }

                return Ok(questionnaire);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpGet("encounters")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalRead)]
        public async Task<ActionResult<IReadOnlyList<ClinicalEncounterDto>>> GetEncounters(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var encounters = await _clinicalRecordQueryService.GetEncountersByPatientIdAsync(patientId, cancellationToken);
                if (encounters is null)
                {
                    return NotFound();
                }

                return Ok(encounters);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalRecordDetailDto>> Create(
            Guid patientId,
            [FromBody] SaveClinicalRecordSnapshotRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicalRecord = await _clinicalRecordCommandService.CreateAsync(patientId, request.ToCommand(), cancellationToken);
                return CreatedAtAction(nameof(GetByPatientId), new { patientId }, clinicalRecord);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPut]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalRecordDetailDto>> Update(
            Guid patientId,
            [FromBody] SaveClinicalRecordSnapshotRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicalRecord = await _clinicalRecordCommandService.UpdateAsync(patientId, request.ToCommand(), cancellationToken);
                if (clinicalRecord is null)
                {
                    return NotFound();
                }

                return Ok(clinicalRecord);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPut("questionnaire")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalMedicalQuestionnaireDto>> UpdateQuestionnaire(
            Guid patientId,
            [FromBody] SaveClinicalMedicalQuestionnaireRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var questionnaire = await _clinicalRecordCommandService.UpdateQuestionnaireAsync(
                    patientId,
                    request.ToCommand(),
                    cancellationToken);

                return Ok(questionnaire);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("encounters")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalEncounterDto>> CreateEncounter(
            Guid patientId,
            [FromBody] CreateClinicalEncounterRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var encounter = await _clinicalRecordCommandService.CreateEncounterAsync(
                    patientId,
                    request.ToCommand(),
                    cancellationToken);

                return CreatedAtAction(nameof(GetEncounters), new { patientId }, encounter);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("notes")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalRecordDetailDto>> AddNote(
            Guid patientId,
            [FromBody] AddClinicalNoteRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicalRecord = await _clinicalRecordCommandService.AddNoteAsync(patientId, request.ToCommand(), cancellationToken);
                return Ok(clinicalRecord);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("diagnoses")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalRecordDetailDto>> AddDiagnosis(
            Guid patientId,
            [FromBody] AddClinicalDiagnosisRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicalRecord = await _clinicalRecordCommandService.AddDiagnosisAsync(patientId, request.ToCommand(), cancellationToken);
                return Ok(clinicalRecord);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("diagnoses/{diagnosisId:guid}/resolve")]
        [Authorize(Policy = AuthorizationPolicies.ClinicalWrite)]
        public async Task<ActionResult<ClinicalRecordDetailDto>> ResolveDiagnosis(
            Guid patientId,
            Guid diagnosisId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clinicalRecord = await _clinicalRecordCommandService.ResolveDiagnosisAsync(patientId, diagnosisId, cancellationToken);
                return Ok(clinicalRecord);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(PatientClinicalRecordsController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class SaveClinicalRecordSnapshotRequest
        {
            [MaxLength(2000)]
            public string? MedicalBackgroundSummary { get; set; }

            [MaxLength(2000)]
            public string? CurrentMedicationsSummary { get; set; }

            public List<ClinicalAllergyRequest>? Allergies { get; set; } = new();

            public SaveClinicalRecordSnapshotCommand ToCommand()
            {
                return new SaveClinicalRecordSnapshotCommand(
                    MedicalBackgroundSummary,
                    CurrentMedicationsSummary,
                    (Allergies ?? new List<ClinicalAllergyRequest>())
                        .Select(allergy => new ClinicalAllergyInput(
                            allergy.Substance,
                            allergy.ReactionSummary,
                            allergy.Notes))
                        .ToList());
            }
        }

        public sealed class ClinicalAllergyRequest
        {
            [Required]
            [MaxLength(150)]
            public string Substance { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? ReactionSummary { get; set; }

            [MaxLength(500)]
            public string? Notes { get; set; }
        }

        public sealed class AddClinicalNoteRequest : IValidatableObject
        {
            [Required]
            [MaxLength(2000)]
            public string NoteText { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(NoteText))
                {
                    yield return new ValidationResult("Clinical note text is required.", new[] { nameof(NoteText) });
                }
            }

            public AddClinicalNoteCommand ToCommand()
            {
                return new AddClinicalNoteCommand(NoteText);
            }
        }

        public sealed class AddClinicalDiagnosisRequest : IValidatableObject
        {
            [Required]
            [MaxLength(250)]
            public string DiagnosisText { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Notes { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(DiagnosisText))
                {
                    yield return new ValidationResult("Diagnosis text is required.", new[] { nameof(DiagnosisText) });
                }
            }

            public AddClinicalDiagnosisCommand ToCommand()
            {
                return new AddClinicalDiagnosisCommand(DiagnosisText, Notes);
            }
        }

        [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
        public sealed class CreateClinicalEncounterRequest : IValidatableObject
        {
            public DateTime OccurredAtUtc { get; set; }

            [Required]
            [MaxLength(ClinicalEncounter.ChiefComplaintMaxLength)]
            public string ChiefComplaint { get; set; } = string.Empty;

            [Required]
            [MaxLength(32)]
            public string ConsultationType { get; set; } = string.Empty;

            public decimal? TemperatureC { get; set; }

            public int? BloodPressureSystolic { get; set; }

            public int? BloodPressureDiastolic { get; set; }

            public decimal? WeightKg { get; set; }

            public decimal? HeightCm { get; set; }

            public int? RespiratoryRatePerMinute { get; set; }

            public int? HeartRateBpm { get; set; }

            [MaxLength(2000)]
            public string? NoteText { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (OccurredAtUtc == default)
                {
                    yield return new ValidationResult(
                        "Clinical encounter occurrence date/time is required.",
                        new[] { nameof(OccurredAtUtc) });
                }

                if (string.IsNullOrWhiteSpace(ChiefComplaint))
                {
                    yield return new ValidationResult(
                        "Clinical encounter chief complaint is required.",
                        new[] { nameof(ChiefComplaint) });
                }

                if (!TryParseEnumName<ClinicalEncounterConsultationType>(ConsultationType, out _))
                {
                    yield return new ValidationResult(
                        "Clinical encounter consultation type must be one of: Treatment, Urgency, Other.",
                        new[] { nameof(ConsultationType) });
                }

                if (!IsInRange(TemperatureC, 30.0m, 45.0m))
                {
                    yield return new ValidationResult(
                        "Temperature must be between 30.0 and 45.0 Celsius.",
                        new[] { nameof(TemperatureC) });
                }

                if (!IsInRange(BloodPressureSystolic, 50, 260))
                {
                    yield return new ValidationResult(
                        "Blood pressure systolic value must be between 50 and 260.",
                        new[] { nameof(BloodPressureSystolic) });
                }

                if (!IsInRange(BloodPressureDiastolic, 30, 180))
                {
                    yield return new ValidationResult(
                        "Blood pressure diastolic value must be between 30 and 180.",
                        new[] { nameof(BloodPressureDiastolic) });
                }

                if (BloodPressureSystolic.HasValue != BloodPressureDiastolic.HasValue)
                {
                    yield return new ValidationResult(
                        "Blood pressure requires both systolic and diastolic values.",
                        new[] { nameof(BloodPressureSystolic), nameof(BloodPressureDiastolic) });
                }
                else if (BloodPressureSystolic.HasValue
                    && BloodPressureDiastolic.HasValue
                    && BloodPressureDiastolic.Value >= BloodPressureSystolic.Value)
                {
                    yield return new ValidationResult(
                        "Blood pressure diastolic value must be lower than systolic value.",
                        new[] { nameof(BloodPressureDiastolic) });
                }

                if (!IsInRange(WeightKg, 0.5m, 500.0m))
                {
                    yield return new ValidationResult(
                        "Weight must be between 0.5 and 500.0 kilograms.",
                        new[] { nameof(WeightKg) });
                }

                if (!IsInRange(HeightCm, 30.0m, 250.0m))
                {
                    yield return new ValidationResult(
                        "Height must be between 30.0 and 250.0 centimeters.",
                        new[] { nameof(HeightCm) });
                }

                if (!IsInRange(RespiratoryRatePerMinute, 5, 80))
                {
                    yield return new ValidationResult(
                        "Respiratory rate must be between 5 and 80 per minute.",
                        new[] { nameof(RespiratoryRatePerMinute) });
                }

                if (!IsInRange(HeartRateBpm, 20, 240))
                {
                    yield return new ValidationResult(
                        "Heart rate must be between 20 and 240 BPM.",
                        new[] { nameof(HeartRateBpm) });
                }
            }

            public CreateClinicalEncounterCommand ToCommand()
            {
                return new CreateClinicalEncounterCommand(
                    OccurredAtUtc,
                    ChiefComplaint,
                    ParseEnumName<ClinicalEncounterConsultationType>(
                        ConsultationType,
                        nameof(ConsultationType)),
                    TemperatureC,
                    BloodPressureSystolic,
                    BloodPressureDiastolic,
                    WeightKg,
                    HeightCm,
                    RespiratoryRatePerMinute,
                    HeartRateBpm,
                    NoteText);
            }
        }

        [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
        public sealed class SaveClinicalMedicalQuestionnaireRequest : IValidatableObject
        {
            [Required]
            public List<ClinicalMedicalAnswerRequest>? Answers { get; set; } = new();

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (Answers is null)
                {
                    yield return new ValidationResult(
                        "Medical questionnaire answers are required.",
                        new[] { nameof(Answers) });
                    yield break;
                }

                var seenQuestionKeys = new HashSet<string>(StringComparer.Ordinal);
                for (var index = 0; index < Answers.Count; index++)
                {
                    var answer = Answers[index];
                    if (answer is null)
                    {
                        yield return new ValidationResult(
                            "Medical questionnaire answer entries cannot be null.",
                            new[] { $"{nameof(Answers)}[{index}]" });
                        continue;
                    }

                    var normalizedQuestionKey = answer.QuestionKey?.Trim();
                    if (string.IsNullOrWhiteSpace(normalizedQuestionKey))
                    {
                        yield return new ValidationResult(
                            "Medical questionnaire question key is required.",
                            new[] { $"{nameof(Answers)}[{index}].{nameof(ClinicalMedicalAnswerRequest.QuestionKey)}" });
                    }
                    else if (!ClinicalMedicalQuestionnaireCatalog.IsAllowedQuestionKey(normalizedQuestionKey))
                    {
                        yield return new ValidationResult(
                            "Medical questionnaire question key is not supported.",
                            new[] { $"{nameof(Answers)}[{index}].{nameof(ClinicalMedicalAnswerRequest.QuestionKey)}" });
                    }
                    else if (!seenQuestionKeys.Add(normalizedQuestionKey))
                    {
                        yield return new ValidationResult(
                            "Medical questionnaire answers cannot contain duplicate question keys.",
                            new[] { $"{nameof(Answers)}[{index}].{nameof(ClinicalMedicalAnswerRequest.QuestionKey)}" });
                    }

                    if (!TryParseEnumName<ClinicalMedicalAnswerValue>(answer.Answer, out _))
                    {
                        yield return new ValidationResult(
                            "Medical questionnaire answer must be one of: Unknown, Yes, No.",
                            new[] { $"{nameof(Answers)}[{index}].{nameof(ClinicalMedicalAnswerRequest.Answer)}" });
                    }

                    if (answer.Details is not null && answer.Details.Length > ClinicalMedicalAnswer.DetailsMaxLength)
                    {
                        yield return new ValidationResult(
                            $"Medical questionnaire details must not exceed {ClinicalMedicalAnswer.DetailsMaxLength} characters.",
                            new[] { $"{nameof(Answers)}[{index}].{nameof(ClinicalMedicalAnswerRequest.Details)}" });
                    }
                }
            }

            public SaveClinicalMedicalQuestionnaireCommand ToCommand()
            {
                return new SaveClinicalMedicalQuestionnaireCommand(
                    (Answers ?? new List<ClinicalMedicalAnswerRequest>())
                        .Where(answer => answer is not null)
                        .Select(answer => new SaveClinicalMedicalAnswerCommand(
                            ClinicalMedicalQuestionnaireCatalog.NormalizeQuestionKey(answer.QuestionKey),
                            ParseEnumName<ClinicalMedicalAnswerValue>(answer.Answer, nameof(ClinicalMedicalAnswerRequest.Answer)),
                            answer.Details))
                        .ToList());
            }
        }

        [JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
        public sealed class ClinicalMedicalAnswerRequest
        {
            [Required]
            [MaxLength(80)]
            public string QuestionKey { get; set; } = string.Empty;

            [Required]
            [MaxLength(20)]
            public string Answer { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Details { get; set; }
        }

        private static TEnum ParseEnumName<TEnum>(string? value, string propertyName)
            where TEnum : struct, Enum
        {
            if (TryParseEnumName<TEnum>(value, out var parsed))
            {
                return parsed;
            }

            throw new ArgumentException($"{propertyName} has an unsupported value.", propertyName);
        }

        private static bool TryParseEnumName<TEnum>(string? value, out TEnum parsed)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                parsed = default;
                return false;
            }

            var normalized = value.Trim();
            foreach (var candidate in Enum.GetValues<TEnum>())
            {
                if (string.Equals(candidate.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
                {
                    parsed = candidate;
                    return true;
                }
            }

            parsed = default;
            return false;
        }

        private static bool IsInRange(decimal? value, decimal minimum, decimal maximum)
        {
            return !value.HasValue || (value.Value >= minimum && value.Value <= maximum);
        }

        private static bool IsInRange(int? value, int minimum, int maximum)
        {
            return !value.HasValue || (value.Value >= minimum && value.Value <= maximum);
        }
    }
}
