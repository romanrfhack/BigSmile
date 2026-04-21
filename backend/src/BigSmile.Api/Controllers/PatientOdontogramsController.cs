using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.Odontograms.Commands;
using BigSmile.Application.Features.Odontograms.Dtos;
using BigSmile.Application.Features.Odontograms.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/odontogram")]
    public class PatientOdontogramsController : ControllerBase
    {
        private readonly IOdontogramCommandService _odontogramCommandService;
        private readonly IOdontogramQueryService _odontogramQueryService;

        public PatientOdontogramsController(
            IOdontogramCommandService odontogramCommandService,
            IOdontogramQueryService odontogramQueryService)
        {
            _odontogramCommandService = odontogramCommandService ?? throw new ArgumentNullException(nameof(odontogramCommandService));
            _odontogramQueryService = odontogramQueryService ?? throw new ArgumentNullException(nameof(odontogramQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.OdontogramRead)]
        public async Task<ActionResult<OdontogramDetailDto>> GetByPatientId(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var odontogram = await _odontogramQueryService.GetByPatientIdAsync(patientId, cancellationToken);
                if (odontogram is null)
                {
                    return NotFound();
                }

                return Ok(odontogram);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.OdontogramWrite)]
        public async Task<ActionResult<OdontogramDetailDto>> Create(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var odontogram = await _odontogramCommandService.CreateAsync(patientId, cancellationToken);
                return CreatedAtAction(nameof(GetByPatientId), new { patientId }, odontogram);
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

        [HttpPut("teeth/{toothCode}")]
        [Authorize(Policy = AuthorizationPolicies.OdontogramWrite)]
        public async Task<ActionResult<OdontogramDetailDto>> UpdateToothStatus(
            Guid patientId,
            string toothCode,
            [FromBody] UpdateToothStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var odontogram = await _odontogramCommandService.UpdateToothStatusAsync(
                    patientId,
                    request.ToCommand(toothCode),
                    cancellationToken);

                if (odontogram is null)
                {
                    return NotFound();
                }

                return Ok(odontogram);
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
            ModelState.AddModelError(nameof(PatientOdontogramsController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class UpdateToothStatusRequest : IValidatableObject
        {
            [Required]
            public string Status { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Status))
                {
                    yield return new ValidationResult("Tooth status is required.", new[] { nameof(Status) });
                }
            }

            public UpdateOdontogramToothStatusCommand ToCommand(string toothCode)
            {
                return new UpdateOdontogramToothStatusCommand(toothCode, Status);
            }
        }
    }
}
