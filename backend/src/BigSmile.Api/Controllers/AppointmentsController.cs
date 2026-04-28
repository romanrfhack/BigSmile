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
        private readonly IAppointmentReminderLogCommandService _reminderLogCommandService;
        private readonly IAppointmentReminderLogQueryService _reminderLogQueryService;

        public AppointmentsController(
            IAppointmentCommandService appointmentCommandService,
            IAppointmentQueryService appointmentQueryService,
            IAppointmentReminderLogCommandService reminderLogCommandService,
            IAppointmentReminderLogQueryService reminderLogQueryService)
        {
            _appointmentCommandService = appointmentCommandService ?? throw new ArgumentNullException(nameof(appointmentCommandService));
            _appointmentQueryService = appointmentQueryService ?? throw new ArgumentNullException(nameof(appointmentQueryService));
            _reminderLogCommandService = reminderLogCommandService ?? throw new ArgumentNullException(nameof(reminderLogCommandService));
            _reminderLogQueryService = reminderLogQueryService ?? throw new ArgumentNullException(nameof(reminderLogQueryService));
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

        [HttpPost("{id:guid}/attended")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> MarkAttended(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.MarkAttendedAsync(id, cancellationToken);
                if (appointment == null)
                {
                    return NotFound();
                }

                return Ok(appointment);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("{id:guid}/no-show")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> MarkNoShow(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.MarkNoShowAsync(id, cancellationToken);
                if (appointment == null)
                {
                    return NotFound();
                }

                return Ok(appointment);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPut("{id:guid}/confirmation")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> ChangeConfirmation(
            Guid id,
            [FromBody] ChangeAppointmentConfirmationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.ChangeConfirmationAsync(
                    id,
                    request.ToCommand(),
                    cancellationToken);
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

        [HttpPut("{id:guid}/manual-reminder")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> ConfigureManualReminder(
            Guid id,
            [FromBody] ConfigureAppointmentManualReminderRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.ConfigureManualReminderAsync(
                    id,
                    request.ToCommand(),
                    cancellationToken);
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

        [HttpPut("{id:guid}/manual-reminder/complete")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentSummaryDto>> CompleteManualReminder(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var appointment = await _appointmentCommandService.CompleteManualReminderAsync(id, cancellationToken);
                if (appointment == null)
                {
                    return NotFound();
                }

                return Ok(appointment);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("{id:guid}/manual-reminder/follow-up")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<ManualReminderFollowUpResultDto>> FollowUpManualReminder(
            Guid id,
            [FromBody] ManualReminderFollowUpRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _appointmentCommandService.FollowUpManualReminderAsync(
                    id,
                    request.ToCommand(),
                    cancellationToken);
                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
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

        [HttpGet("manual-reminders")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingRead)]
        public async Task<ActionResult<IReadOnlyList<AppointmentReminderWorkItemDto>>> ListManualReminders(
            [FromQuery] Guid branchId,
            [FromQuery] bool includeCompleted = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var reminders = await _appointmentQueryService.ListManualRemindersAsync(
                    branchId,
                    includeCompleted,
                    cancellationToken);

                return Ok(reminders);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpGet("{id:guid}/reminder-log")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingRead)]
        public async Task<ActionResult<IReadOnlyList<AppointmentReminderLogEntryDto>>> ListReminderLog(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entries = await _reminderLogQueryService.ListAsync(id, cancellationToken);
                if (entries == null)
                {
                    return NotFound();
                }

                return Ok(entries);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost("{id:guid}/reminder-log")]
        [Authorize(Policy = AuthorizationPolicies.SchedulingWrite)]
        public async Task<ActionResult<AppointmentReminderLogEntryDto>> AddReminderLogEntry(
            Guid id,
            [FromBody] AddAppointmentReminderLogEntryRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var entry = await _reminderLogCommandService.AddAsync(id, request.ToCommand(), cancellationToken);
                if (entry == null)
                {
                    return NotFound();
                }

                return CreatedAtAction(nameof(ListReminderLog), new { id }, entry);
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

        public sealed class ChangeAppointmentConfirmationRequest : IValidatableObject
        {
            [Required]
            public string Status { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Status))
                {
                    yield return new ValidationResult("Appointment confirmation status is required.", new[] { nameof(Status) });
                }
            }

            public ChangeAppointmentConfirmationCommand ToCommand()
            {
                return new ChangeAppointmentConfirmationCommand(Status);
            }
        }

        public sealed class ConfigureAppointmentManualReminderRequest : IValidatableObject
        {
            public bool Required { get; set; }
            public string? Channel { get; set; }
            public DateTime? DueAtUtc { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!Required)
                {
                    yield break;
                }

                if (string.IsNullOrWhiteSpace(Channel))
                {
                    yield return new ValidationResult("Appointment reminder channel is required.", new[] { nameof(Channel) });
                }

                if (!DueAtUtc.HasValue)
                {
                    yield return new ValidationResult("Manual reminder due date/time is required.", new[] { nameof(DueAtUtc) });
                }
            }

            public ConfigureAppointmentManualReminderCommand ToCommand()
            {
                return new ConfigureAppointmentManualReminderCommand(Required, Channel, DueAtUtc);
            }
        }

        public sealed class AddAppointmentReminderLogEntryRequest : IValidatableObject
        {
            [Required]
            public string Channel { get; set; } = string.Empty;

            [Required]
            public string Outcome { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Notes { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Channel))
                {
                    yield return new ValidationResult("Appointment reminder channel is required.", new[] { nameof(Channel) });
                }

                if (string.IsNullOrWhiteSpace(Outcome))
                {
                    yield return new ValidationResult("Appointment reminder outcome is required.", new[] { nameof(Outcome) });
                }
            }

            public AddAppointmentReminderLogEntryCommand ToCommand()
            {
                return new AddAppointmentReminderLogEntryCommand(Channel, Outcome, Notes);
            }
        }

        public sealed class ManualReminderFollowUpRequest : IValidatableObject
        {
            [Required]
            public string Channel { get; set; } = string.Empty;

            [Required]
            public string Outcome { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Notes { get; set; }

            public bool CompleteReminder { get; set; }
            public bool ConfirmAppointment { get; set; }
            public Guid? ReminderTemplateId { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Channel))
                {
                    yield return new ValidationResult("Appointment reminder channel is required.", new[] { nameof(Channel) });
                }

                if (string.IsNullOrWhiteSpace(Outcome))
                {
                    yield return new ValidationResult("Appointment reminder outcome is required.", new[] { nameof(Outcome) });
                }

                if (ReminderTemplateId == Guid.Empty)
                {
                    yield return new ValidationResult("Reminder template reference must be a non-empty identifier when provided.", new[] { nameof(ReminderTemplateId) });
                }
            }

            public ManualReminderFollowUpCommand ToCommand()
            {
                return new ManualReminderFollowUpCommand(
                    Channel,
                    Outcome,
                    Notes,
                    CompleteReminder,
                    ConfirmAppointment,
                    ReminderTemplateId);
            }
        }
    }
}
