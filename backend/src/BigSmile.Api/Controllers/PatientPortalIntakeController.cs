using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientIntakes.Dtos;
using BigSmile.Application.Features.PatientIntakes.Services;
using BigSmile.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patient-portal/intake")]
    [Authorize(Policy = PatientPortalAuthenticationDefaults.PatientIntakeSelfPolicy)]
    public sealed class PatientPortalIntakeController : ControllerBase
    {
        private readonly IPatientIntakeSelfService _intakeService;

        public PatientPortalIntakeController(IPatientIntakeSelfService intakeService)
        {
            _intakeService = intakeService ?? throw new ArgumentNullException(nameof(intakeService));
        }

        [HttpGet]
        public async Task<ActionResult<PatientIntakeDto>> GetCurrent(
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            PatientIntakeReadResult result;
            if (PatientPortalClaims.TryGetSessionIdentity(User, out var patientIdentity))
            {
                result = await _intakeService.GetCurrentAsync(
                    patientIdentity,
                    cancellationToken);
            }
            else if (PatientPortalClaims.TryGetIntakeSessionIdentity(User, out var intakeIdentity))
            {
                result = await _intakeService.GetCurrentAsync(
                    intakeIdentity,
                    cancellationToken);
            }
            else
            {
                return Unauthorized();
            }

            return result.Failure switch
            {
                PatientIntakeReadFailure.None => Ok(result.Intake),
                PatientIntakeReadFailure.SessionInvalid => Unauthorized(),
                _ => BuildMissingProblem()
            };
        }

        [HttpPost]
        public async Task<ActionResult<PatientIntakeDto>> Create(
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();
            if (!PatientPortalClaims.TryGetSessionIdentity(User, out var identity))
            {
                return Forbid();
            }

            try
            {
                var result = await _intakeService.CreateAsync(identity, cancellationToken);
                return result.Failure switch
                {
                    PatientIntakeCreateFailure.None => StatusCode(
                        StatusCodes.Status201Created,
                        result.Intake),
                    PatientIntakeCreateFailure.SessionInvalid => Unauthorized(),
                    PatientIntakeCreateFailure.ActiveDraftExists => BuildConflictProblem(
                        "A current patient intake draft already exists."),
                    _ => BuildConflictProblem(
                        "The patient intake draft could not be created because its state changed.")
                };
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
        public async Task<ActionResult<PatientIntakeSaveResponseDto>> Save(
            [FromBody] SavePatientIntakeRequest request,
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            try
            {
                PatientIntakeSaveResult result;
                if (PatientPortalClaims.TryGetSessionIdentity(User, out var patientIdentity))
                {
                    result = await _intakeService.SaveAsync(
                        patientIdentity,
                        request.ToCommand(),
                        GetCorrelationId(),
                        cancellationToken);
                }
                else if (PatientPortalClaims.TryGetIntakeSessionIdentity(User, out var intakeIdentity))
                {
                    result = await _intakeService.SaveAsync(
                        intakeIdentity,
                        request.ToCommand(),
                        GetCorrelationId(),
                        cancellationToken);
                }
                else
                {
                    return Unauthorized();
                }

                return result.Failure switch
                {
                    PatientIntakeSaveFailure.None => Ok(new PatientIntakeSaveResponseDto(
                        result.Intake!,
                        result.Changed)),
                    PatientIntakeSaveFailure.SessionInvalid => Unauthorized(),
                    PatientIntakeSaveFailure.Missing => BuildMissingProblem(),
                    PatientIntakeSaveFailure.Expired => BuildConflictProblem(
                        "The current patient intake draft has expired. Start a new draft."),
                    _ => BuildConflictProblem(
                        "The patient intake draft changed. Reload it before saving again.")
                };
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

        private ObjectResult BuildMissingProblem()
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Patient intake draft not found.",
                Detail = "No current patient intake draft is available for this session."
            });
        }

        private ObjectResult BuildConflictProblem(string detail)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Patient intake draft conflict.",
                Detail = detail
            });
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(SavePatientIntakeRequest), message);
            return ValidationProblem(ModelState);
        }

        private string GetCorrelationId()
        {
            return string.IsNullOrWhiteSpace(HttpContext.TraceIdentifier)
                ? Guid.NewGuid().ToString("N")
                : HttpContext.TraceIdentifier;
        }

        private void SetNoStoreHeaders()
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";
        }

        public sealed class SavePatientIntakeRequest : IValidatableObject
        {
            [MaxLength(PatientIntake.NameMaxLength)]
            public string? FirstName { get; set; }

            [MaxLength(PatientIntake.NameMaxLength)]
            public string? LastName { get; set; }

            public DateOnly? DateOfBirth { get; set; }

            [MaxLength(20)]
            public string? Sex { get; set; } = PatientSex.Unspecified.ToString();

            [MaxLength(PatientIntake.DemographicMaxLength)]
            public string? Occupation { get; set; }

            [MaxLength(20)]
            public string? MaritalStatus { get; set; } = PatientMaritalStatus.Unspecified.ToString();

            [MaxLength(PatientIntake.DemographicMaxLength)]
            public string? ReferredBy { get; set; }

            [MaxLength(PatientIntake.PhoneMaxLength)]
            public string? PreferredPhone { get; set; }

            [MaxLength(PatientIntake.PhoneMaxLength)]
            public string? MobilePhone { get; set; }

            [MaxLength(PatientIntake.PhoneMaxLength)]
            public string? HomePhone { get; set; }

            [MaxLength(PatientIntake.PhoneMaxLength)]
            public string? WorkPhone { get; set; }

            [EmailAddress]
            [MaxLength(PatientIntake.EmailMaxLength)]
            public string? Email { get; set; }

            [MaxLength(PatientIntake.NameMaxLength)]
            public string? ResponsiblePartyName { get; set; }

            [MaxLength(PatientIntake.DemographicMaxLength)]
            public string? ResponsiblePartyRelationship { get; set; }

            [MaxLength(PatientIntake.PhoneMaxLength)]
            public string? ResponsiblePartyPhone { get; set; }

            [MaxLength(PatientIntake.ReasonForVisitMaxLength)]
            public string? ReasonForVisit { get; set; }

            [Required]
            public List<SavePatientIntakeMedicalAnswerRequest> MedicalAnswers { get; set; } = new();

            [Required]
            [MaxLength(256)]
            public string ConcurrencyToken { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!TryParseOptionalEnum(Sex, out PatientSex _))
                {
                    yield return new ValidationResult(
                        "Sex must be one of: Unspecified, Female, Male, Other.",
                        new[] { nameof(Sex) });
                }

                if (!TryParseOptionalEnum(MaritalStatus, out PatientMaritalStatus _))
                {
                    yield return new ValidationResult(
                        "Marital status must be one of: Unspecified, Single, Married, Divorced, Widowed, Other.",
                        new[] { nameof(MaritalStatus) });
                }

                if (DateOfBirth.HasValue && DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    yield return new ValidationResult(
                        "Date of birth cannot be in the future.",
                        new[] { nameof(DateOfBirth) });
                }

                if ((HasValue(ResponsiblePartyRelationship) || HasValue(ResponsiblePartyPhone)) &&
                    !HasValue(ResponsiblePartyName))
                {
                    yield return new ValidationResult(
                        "Responsible party name is required when relationship or phone is provided.",
                        new[] { nameof(ResponsiblePartyName) });
                }

                if (MedicalAnswers is null)
                {
                    yield return new ValidationResult(
                        "Medical answers are required.",
                        new[] { nameof(MedicalAnswers) });
                    yield break;
                }

                for (var index = 0; index < MedicalAnswers.Count; index++)
                {
                    var answer = MedicalAnswers[index];
                    if (answer is null || !TryParseRequiredEnum(answer.Answer, out ClinicalMedicalAnswerValue _))
                    {
                        yield return new ValidationResult(
                            "Medical answer must be one of: Unknown, Yes, No.",
                            new[] { $"{nameof(MedicalAnswers)}[{index}].{nameof(SavePatientIntakeMedicalAnswerRequest.Answer)}" });
                    }
                }
            }

            public SavePatientIntakeDraftCommand ToCommand()
            {
                if (MedicalAnswers is null)
                {
                    throw new ArgumentException(
                        "Medical answers are required.",
                        nameof(MedicalAnswers));
                }

                return new SavePatientIntakeDraftCommand(
                    FirstName,
                    LastName,
                    DateOfBirth,
                    ParseOptionalEnum<PatientSex>(Sex, nameof(Sex)),
                    Occupation,
                    ParseOptionalEnum<PatientMaritalStatus>(MaritalStatus, nameof(MaritalStatus)),
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
                    MedicalAnswers.Select(answer => new SavePatientIntakeMedicalAnswerCommand(
                        answer.QuestionKey,
                        ParseRequiredEnum<ClinicalMedicalAnswerValue>(answer.Answer, nameof(answer.Answer)),
                        answer.Details)).ToArray(),
                    ConcurrencyToken);
            }
        }

        public sealed class SavePatientIntakeMedicalAnswerRequest
        {
            [Required]
            [MaxLength(ClinicalMedicalQuestionnaireCatalog.QuestionKeyMaxLength)]
            public string QuestionKey { get; set; } = string.Empty;

            [Required]
            [MaxLength(20)]
            public string Answer { get; set; } = string.Empty;

            [MaxLength(ClinicalMedicalAnswer.DetailsMaxLength)]
            public string? Details { get; set; }
        }

        private static bool HasValue(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryParseOptionalEnum<TEnum>(string? value, out TEnum parsed)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                parsed = default;
                return true;
            }

            return TryParseRequiredEnum(value, out parsed);
        }

        private static bool TryParseRequiredEnum<TEnum>(string? value, out TEnum parsed)
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

        private static TEnum ParseOptionalEnum<TEnum>(string? value, string propertyName)
            where TEnum : struct, Enum
        {
            return TryParseOptionalEnum(value, out TEnum parsed)
                ? parsed
                : throw new ArgumentException($"{propertyName} has an unsupported value.", propertyName);
        }

        private static TEnum ParseRequiredEnum<TEnum>(string? value, string propertyName)
            where TEnum : struct, Enum
        {
            return TryParseRequiredEnum(value, out TEnum parsed)
                ? parsed
                : throw new ArgumentException($"{propertyName} has an unsupported value.", propertyName);
        }
    }
}
