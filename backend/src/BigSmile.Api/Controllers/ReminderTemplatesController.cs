using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Features.Scheduling.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/reminder-templates")]
    public sealed class ReminderTemplatesController : ControllerBase
    {
        private const int NameMaxLength = 120;
        private const int BodyMaxLength = 1000;

        private readonly IReminderTemplateCommandService _commandService;
        private readonly IReminderTemplateQueryService _queryService;

        public ReminderTemplatesController(
            IReminderTemplateCommandService commandService,
            IReminderTemplateQueryService queryService)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.SchedulingRead)]
        public async Task<ActionResult<IReadOnlyList<ReminderTemplateDto>>> List(
            [FromQuery] bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var templates = await _queryService.ListAsync(includeInactive, cancellationToken);
                return Ok(templates);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<ReminderTemplateDto>> Create(
            [FromBody] SaveReminderTemplateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var template = await _commandService.CreateAsync(request.ToCreateCommand(), cancellationToken);
                return CreatedAtAction(nameof(List), new { includeInactive = false }, template);
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
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<ReminderTemplateDto>> Update(
            Guid id,
            [FromBody] SaveReminderTemplateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var template = await _commandService.UpdateAsync(id, request.ToUpdateCommand(), cancellationToken);
                if (template == null)
                {
                    return NotFound();
                }

                return Ok(template);
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
        public async Task<IActionResult> Deactivate(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var deactivated = await _commandService.DeactivateAsync(id, cancellationToken);
                return deactivated ? NoContent() : NotFound();
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("{id:guid}/preview")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingRead)]
        public async Task<ActionResult<ReminderTemplatePreviewDto>> Preview(
            Guid id,
            [FromBody] PreviewReminderTemplateRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var preview = await _queryService.PreviewAsync(id, request.AppointmentId, cancellationToken);
                if (preview == null)
                {
                    return NotFound();
                }

                return Ok(preview);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(ReminderTemplatesController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class SaveReminderTemplateRequest : IValidatableObject
        {
            public string Name { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                var normalizedName = Name?.Trim();
                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    yield return new ValidationResult("Reminder template name is required.", new[] { nameof(Name) });
                }
                else if (normalizedName.Length > NameMaxLength)
                {
                    yield return new ValidationResult(
                        $"Reminder template name must be {NameMaxLength} characters or fewer.",
                        new[] { nameof(Name) });
                }

                var normalizedBody = Body?.Trim();
                if (string.IsNullOrWhiteSpace(normalizedBody))
                {
                    yield return new ValidationResult("Reminder template body is required.", new[] { nameof(Body) });
                }
                else if (normalizedBody.Length > BodyMaxLength)
                {
                    yield return new ValidationResult(
                        $"Reminder template body must be {BodyMaxLength} characters or fewer.",
                        new[] { nameof(Body) });
                }
            }

            public CreateReminderTemplateCommand ToCreateCommand()
            {
                return new CreateReminderTemplateCommand(Name, Body);
            }

            public UpdateReminderTemplateCommand ToUpdateCommand()
            {
                return new UpdateReminderTemplateCommand(Name, Body);
            }
        }

        public sealed class PreviewReminderTemplateRequest : IValidatableObject
        {
            [Required]
            public Guid AppointmentId { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (AppointmentId == Guid.Empty)
                {
                    yield return new ValidationResult("Appointment is required.", new[] { nameof(AppointmentId) });
                }
            }
        }
    }
}
