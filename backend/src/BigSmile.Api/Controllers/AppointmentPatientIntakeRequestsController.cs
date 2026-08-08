using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientIntakeRequests.Dtos;
using BigSmile.Application.Features.PatientIntakeRequests.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/appointments/{appointmentId:guid}/patient-intake-request")]
    [Authorize(Policy = AuthorizationPolicies.PatientPortalIntakeRequest)]
    public sealed class AppointmentPatientIntakeRequestsController : ControllerBase
    {
        private readonly IAppointmentPatientIntakeRequestService _requestService;

        public AppointmentPatientIntakeRequestsController(
            IAppointmentPatientIntakeRequestService requestService)
        {
            _requestService = requestService ?? throw new ArgumentNullException(nameof(requestService));
        }

        [HttpGet]
        public async Task<ActionResult<AppointmentPatientIntakeRequestStatusDto>> GetStatus(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            try
            {
                var status = await _requestService.GetStatusAsync(
                    appointmentId,
                    cancellationToken);
                return status is null ? NotFound() : Ok(status);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<PreparedAppointmentPatientIntakeRequestDto>> Prepare(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            try
            {
                var result = await _requestService.PrepareAsync(
                    appointmentId,
                    GetCorrelationId(),
                    cancellationToken);

                return result.Failure switch
                {
                    AppointmentPatientIntakeRequestFailure.None => Ok(result.PreparedRequest),
                    AppointmentPatientIntakeRequestFailure.NotFound => NotFound(),
                    AppointmentPatientIntakeRequestFailure.AlreadyCompleted => BuildConflictProblem(
                        "patient_intake.already_submitted",
                        "The patient already completed this medical-history intake."),
                    AppointmentPatientIntakeRequestFailure.RecoveryRequired => BuildConflictProblem(
                        "patient_portal.recovery_required",
                        "The patient portal account requires assisted recovery before access can be sent."),
                    _ => BuildConflictProblem(
                        "patient_intake.request_unavailable",
                        "Patient intake access is not available for this appointment.")
                };
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        private ObjectResult BuildConflictProblem(string code, string detail)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Patient intake request conflict.",
                Detail = detail,
                Extensions = { ["code"] = code }
            });
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(AppointmentPatientIntakeRequestsController), message);
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
    }
}
