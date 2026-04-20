using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Application.Features.ClinicalRecords.Queries;
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
    }
}
