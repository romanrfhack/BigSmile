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
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentCommandService _appointmentCommandService;
        private readonly IAppointmentQueryService _appointmentQueryService;

        public AppointmentsController(
            IAppointmentCommandService appointmentCommandService,
            IAppointmentQueryService appointmentQueryService)
        {
            _appointmentCommandService = appointmentCommandService ?? throw new ArgumentNullException(nameof(appointmentCommandService));
            _appointmentQueryService = appointmentQueryService ?? throw new ArgumentNullException(nameof(appointmentQueryService));
        }

        [HttpGet("calendar")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingRead)]
        public async Task<ActionResult<CalendarViewDto>> GetCalendar(
            [FromQuery] Guid branchId,
            [FromQuery] DateOnly startDate,
            [FromQuery][Range(1, 7)] int days = 1,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var calendar = await _appointmentQueryService.GetCalendarAsync(branchId, startDate, days, cancellationToken);
                return Ok(calendar);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> Create(
            [FromBody] CreateAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.CreateAsync(request.ToCommand(), cancellationToken);
                return CreatedAtAction(nameof(GetCalendar), new { branchId = appointment.BranchId, startDate = DateOnly.FromDateTime(appointment.StartsAt), days = 1 }, appointment);
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
        public async Task<ActionResult<AppointmentSummaryDto>> Update(
            Guid id,
            [FromBody] UpdateAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.UpdateAsync(id, request.ToCommand(), cancellationToken);
                if (appointment == null)
                {
                    return NotFound();
                }

                return Ok(appointment);
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

        [HttpPost("{id:guid}/reschedule")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> Reschedule(
            Guid id,
            [FromBody] RescheduleAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.RescheduleAsync(id, request.ToCommand(), cancellationToken);
                if (appointment == null)
                {
                    return NotFound();
                }

                return Ok(appointment);
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

        [HttpPost("{id:guid}/cancel")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> Cancel(
            Guid id,
            [FromBody] CancelAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.CancelAsync(id, request.ToCommand(), cancellationToken);
                if (appointment == null)
                {
                    return NotFound();
                }

                return Ok(appointment);
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
            ModelState.AddModelError(nameof(AppointmentsController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class CreateAppointmentRequest : IValidatableObject
        {
            [Required]
            public Guid BranchId { get; set; }

            [Required]
            public Guid PatientId { get; set; }

            public DateTime StartsAt { get; set; }
            public DateTime EndsAt { get; set; }

            [MaxLength(1000)]
            public string? Notes { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (BranchId == Guid.Empty)
                {
                    yield return new ValidationResult("Branch is required.", new[] { nameof(BranchId) });
                }

                if (PatientId == Guid.Empty)
                {
                    yield return new ValidationResult("Patient is required.", new[] { nameof(PatientId) });
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

            public CreateAppointmentCommand ToCommand()
            {
                return new CreateAppointmentCommand(BranchId, PatientId, StartsAt, EndsAt, Notes);
            }
        }

        public sealed class UpdateAppointmentRequest : IValidatableObject
        {
            [Required]
            public Guid PatientId { get; set; }

            public DateTime StartsAt { get; set; }
            public DateTime EndsAt { get; set; }

            [MaxLength(1000)]
            public string? Notes { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (PatientId == Guid.Empty)
                {
                    yield return new ValidationResult("Patient is required.", new[] { nameof(PatientId) });
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

            public UpdateAppointmentCommand ToCommand()
            {
                return new UpdateAppointmentCommand(PatientId, StartsAt, EndsAt, Notes);
            }
        }

        public sealed class RescheduleAppointmentRequest : IValidatableObject
        {
            public DateTime StartsAt { get; set; }
            public DateTime EndsAt { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
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

            public RescheduleAppointmentCommand ToCommand()
            {
                return new RescheduleAppointmentCommand(StartsAt, EndsAt);
            }
        }

        public sealed class CancelAppointmentRequest
        {
            [MaxLength(500)]
            public string? Reason { get; set; }

            public CancelAppointmentCommand ToCommand()
            {
                return new CancelAppointmentCommand(Reason);
            }
        }
    }
}
