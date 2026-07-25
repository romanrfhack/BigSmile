using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using BigSmile.Application.Features.PatientPortalInvitations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/portal-invitations")]
    [Authorize(Policy = AuthorizationPolicies.PatientPortalInvitationManage)]
    public sealed class PatientPortalInvitationsController : ControllerBase
    {
        private readonly IPatientPortalInvitationCommandService _commandService;
        private readonly IPatientPortalInvitationQueryService _queryService;

        public PatientPortalInvitationsController(
            IPatientPortalInvitationCommandService commandService,
            IPatientPortalInvitationQueryService queryService)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PatientPortalInvitationSummaryDto>>> List(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var invitations = await _queryService.ListAsync(patientId, cancellationToken);
            if (invitations is null)
            {
                return NotFound();
            }

            return Ok(invitations);
        }

        [HttpPost]
        public async Task<ActionResult<IssuedPatientPortalInvitationDto>> Issue(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var invitation = await _commandService.IssueAsync(
                    patientId,
                    GetCorrelationId(),
                    cancellationToken);
                if (invitation is null)
                {
                    return NotFound();
                }

                SetSensitiveResponseHeaders();
                return CreatedAtAction(nameof(List), new { patientId }, invitation);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Patient portal invitation conflict.",
                    Detail = exception.Message
                });
            }
        }

        [HttpDelete("{invitationId:guid}")]
        public async Task<IActionResult> Revoke(
            Guid patientId,
            Guid invitationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var revoked = await _commandService.RevokeAsync(
                    patientId,
                    invitationId,
                    GetCorrelationId(),
                    cancellationToken);
                if (!revoked)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Patient portal invitation conflict.",
                    Detail = exception.Message
                });
            }
        }

        private void SetSensitiveResponseHeaders()
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";
        }

        private string GetCorrelationId()
        {
            return string.IsNullOrWhiteSpace(HttpContext.TraceIdentifier)
                ? Guid.NewGuid().ToString("N")
                : HttpContext.TraceIdentifier;
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(PatientPortalInvitationsController), message);
            return ValidationProblem(ModelState);
        }
    }
}
