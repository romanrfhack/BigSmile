using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.Patients.Commands;
using BigSmile.Application.Features.Patients.Dtos;
using BigSmile.Application.Features.Patients.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientCommandService _patientCommandService;
        private readonly IPatientQueryService _patientQueryService;

        public PatientsController(
            IPatientCommandService patientCommandService,
            IPatientQueryService patientQueryService)
        {
            _patientCommandService = patientCommandService ?? throw new ArgumentNullException(nameof(patientCommandService));
            _patientQueryService = patientQueryService ?? throw new ArgumentNullException(nameof(patientQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.PatientRead)]
        public async Task<ActionResult<IReadOnlyList<PatientSummaryDto>>> Search(
            [FromQuery] string? search,
            [FromQuery] bool includeInactive = false,
            [FromQuery][Range(1, 100)] int take = 25,
            CancellationToken cancellationToken = default)
        {
            var patients = await _patientQueryService.SearchAsync(search, includeInactive, take, cancellationToken);
            return Ok(patients);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = AuthorizationPolicies.PatientRead)]
        public async Task<ActionResult<PatientDetailDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var patient = await _patientQueryService.GetByIdAsync(id, cancellationToken);
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.PatientWrite)]
        public async Task<ActionResult<PatientDetailDto>> Create(
            [FromBody] SavePatientRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var patient = await _patientCommandService.CreateAsync(request.ToCommand(), cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
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

        [HttpPut("{id:guid}")]
        [Authorize(Policy = AuthorizationPolicies.PatientWrite)]
        public async Task<ActionResult<PatientDetailDto>> Update(
            Guid id,
            [FromBody] SavePatientRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var patient = await _patientCommandService.UpdateAsync(id, request.ToCommand(), cancellationToken);
                if (patient == null)
                {
                    return NotFound();
                }

                return Ok(patient);
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
            ModelState.AddModelError(nameof(SavePatientRequest), message);
            return ValidationProblem(ModelState);
        }

        public sealed class SavePatientRequest : IValidatableObject
        {
            [Required]
            [MaxLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [MaxLength(100)]
            public string LastName { get; set; } = string.Empty;

            public DateOnly DateOfBirth { get; set; }

            [MaxLength(40)]
            public string? PrimaryPhone { get; set; }

            [EmailAddress]
            [MaxLength(256)]
            public string? Email { get; set; }

            public bool IsActive { get; set; } = true;

            public bool HasClinicalAlerts { get; set; }

            [MaxLength(500)]
            public string? ClinicalAlertsSummary { get; set; }

            [MaxLength(100)]
            public string? ResponsiblePartyName { get; set; }

            [MaxLength(100)]
            public string? ResponsiblePartyRelationship { get; set; }

            [MaxLength(40)]
            public string? ResponsiblePartyPhone { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (DateOfBirth == default)
                {
                    yield return new ValidationResult("Date of birth is required.", new[] { nameof(DateOfBirth) });
                }

                if (DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    yield return new ValidationResult("Date of birth cannot be in the future.", new[] { nameof(DateOfBirth) });
                }

                if ((HasValue(ResponsiblePartyRelationship) || HasValue(ResponsiblePartyPhone)) &&
                    !HasValue(ResponsiblePartyName))
                {
                    yield return new ValidationResult(
                        "Responsible party name is required when additional responsible party data is provided.",
                        new[] { nameof(ResponsiblePartyName) });
                }
            }

            public SavePatientCommand ToCommand()
            {
                return new SavePatientCommand(
                    FirstName,
                    LastName,
                    DateOfBirth,
                    PrimaryPhone,
                    Email,
                    IsActive,
                    HasClinicalAlerts,
                    ClinicalAlertsSummary,
                    ResponsiblePartyName,
                    ResponsiblePartyRelationship,
                    ResponsiblePartyPhone);
            }

            private static bool HasValue(string? value)
            {
                return !string.IsNullOrWhiteSpace(value);
            }
        }
    }
}
