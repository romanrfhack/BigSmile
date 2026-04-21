using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.TreatmentPlans.Commands;
using BigSmile.Application.Features.TreatmentPlans.Dtos;
using BigSmile.Application.Features.TreatmentPlans.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/treatment-plan")]
    public class PatientTreatmentPlansController : ControllerBase
    {
        private readonly ITreatmentPlanCommandService _treatmentPlanCommandService;
        private readonly ITreatmentPlanQueryService _treatmentPlanQueryService;

        public PatientTreatmentPlansController(
            ITreatmentPlanCommandService treatmentPlanCommandService,
            ITreatmentPlanQueryService treatmentPlanQueryService)
        {
            _treatmentPlanCommandService = treatmentPlanCommandService ?? throw new ArgumentNullException(nameof(treatmentPlanCommandService));
            _treatmentPlanQueryService = treatmentPlanQueryService ?? throw new ArgumentNullException(nameof(treatmentPlanQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.TreatmentPlanRead)]
        public async Task<ActionResult<TreatmentPlanDetailDto>> GetByPatientId(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentPlan = await _treatmentPlanQueryService.GetByPatientIdAsync(patientId, cancellationToken);
                if (treatmentPlan is null)
                {
                    return NotFound();
                }

                return Ok(treatmentPlan);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreatmentPlanWrite)]
        public async Task<ActionResult<TreatmentPlanDetailDto>> Create(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentPlan = await _treatmentPlanCommandService.CreateAsync(patientId, cancellationToken);
                return CreatedAtAction(nameof(GetByPatientId), new { patientId }, treatmentPlan);
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

        [HttpPost("items")]
        [Authorize(Policy = AuthorizationPolicies.TreatmentPlanWrite)]
        public async Task<ActionResult<TreatmentPlanDetailDto>> AddItem(
            Guid patientId,
            [FromBody] AddTreatmentPlanItemRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentPlan = await _treatmentPlanCommandService.AddItemAsync(
                    patientId,
                    request.ToCommand(),
                    cancellationToken);

                if (treatmentPlan is null)
                {
                    return NotFound();
                }

                return Ok(treatmentPlan);
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

        [HttpDelete("items/{itemId:guid}")]
        [Authorize(Policy = AuthorizationPolicies.TreatmentPlanWrite)]
        public async Task<ActionResult<TreatmentPlanDetailDto>> RemoveItem(
            Guid patientId,
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentPlan = await _treatmentPlanCommandService.RemoveItemAsync(patientId, itemId, cancellationToken);
                if (treatmentPlan is null)
                {
                    return NotFound();
                }

                return Ok(treatmentPlan);
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

        [HttpPut("status")]
        [Authorize(Policy = AuthorizationPolicies.TreatmentPlanWrite)]
        public async Task<ActionResult<TreatmentPlanDetailDto>> ChangeStatus(
            Guid patientId,
            [FromBody] ChangeTreatmentPlanStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentPlan = await _treatmentPlanCommandService.ChangeStatusAsync(
                    patientId,
                    request.ToCommand(),
                    cancellationToken);

                if (treatmentPlan is null)
                {
                    return NotFound();
                }

                return Ok(treatmentPlan);
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
            ModelState.AddModelError(nameof(PatientTreatmentPlansController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class AddTreatmentPlanItemRequest : IValidatableObject
        {
            [Required]
            [MaxLength(200)]
            public string Title { get; set; } = string.Empty;

            [MaxLength(100)]
            public string? Category { get; set; }

            [Range(1, int.MaxValue)]
            public int Quantity { get; set; } = 1;

            [MaxLength(500)]
            public string? Notes { get; set; }

            [MaxLength(2)]
            public string? ToothCode { get; set; }

            [MaxLength(1)]
            public string? SurfaceCode { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Title))
                {
                    yield return new ValidationResult("Treatment item title is required.", new[] { nameof(Title) });
                }

                if (!string.IsNullOrWhiteSpace(SurfaceCode) && string.IsNullOrWhiteSpace(ToothCode))
                {
                    yield return new ValidationResult("SurfaceCode requires ToothCode.", new[] { nameof(SurfaceCode), nameof(ToothCode) });
                }
            }

            public AddTreatmentPlanItemCommand ToCommand()
            {
                return new AddTreatmentPlanItemCommand(
                    Title,
                    Category,
                    Quantity,
                    Notes,
                    ToothCode,
                    SurfaceCode);
            }
        }

        public sealed class ChangeTreatmentPlanStatusRequest : IValidatableObject
        {
            [Required]
            public string Status { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Status))
                {
                    yield return new ValidationResult("Treatment plan status is required.", new[] { nameof(Status) });
                }
            }

            public ChangeTreatmentPlanStatusCommand ToCommand()
            {
                return new ChangeTreatmentPlanStatusCommand(Status);
            }
        }
    }
}
