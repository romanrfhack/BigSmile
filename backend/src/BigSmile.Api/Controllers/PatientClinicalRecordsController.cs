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
    }
}
