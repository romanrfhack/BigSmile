using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentBlocksController : ControllerBase
    {
        private readonly IAppointmentBlockCommandService _appointmentBlockCommandService;

        public AppointmentBlocksController(IAppointmentBlockCommandService appointmentBlockCommandService)
        {
            _appointmentBlockCommandService = appointmentBlockCommandService ?? throw new ArgumentNullException(nameof(appointmentBlockCommandService));
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentBlockSummaryDto>> Create(
            [FromBody] CreateAppointmentBlockRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointmentBlock = await _appointmentBlockCommandService.CreateAsync(request.ToCommand(), cancellationToken);
                return Created($"/api/appointment-blocks/{appointmentBlock.Id}", appointmentBlock);
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

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var deleted = await _appointmentBlockCommandService.DeleteAsync(id, cancellationToken);
                return deleted ? NoContent() : NotFound();
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(AppointmentBlocksController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class CreateAppointmentBlockRequest : IValidatableObject
        {
            [Required]
            public Guid BranchId { get; set; }

            public DateTime StartsAt { get; set; }
            public DateTime EndsAt { get; set; }

            [MaxLength(200)]
            public string? Label { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (BranchId == Guid.Empty)
                {
                    yield return new ValidationResult("Branch is required.", new[] { nameof(BranchId) });
                }

                if (StartsAt == default)
                {
                    yield return new ValidationResult("Start time is required.", new[] { nameof(StartsAt) });
                }

                if (EndsAt == default)
                {
                    yield return new ValidationResult("End time is required.", new[] { nameof(EndsAt) });
                }

                if (EndsAt <= StartsAt)
                {
                    yield return new ValidationResult("End time must be after the start time.", new[] { nameof(EndsAt) });
                }
            }

            public CreateAppointmentBlockCommand ToCommand()
            {
                return new CreateAppointmentBlockCommand(BranchId, StartsAt, EndsAt, Label);
            }
        }
    }
}
