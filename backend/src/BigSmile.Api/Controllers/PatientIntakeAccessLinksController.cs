using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Commands;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patient-intake-links")]
    [Authorize(Policy = AuthorizationPolicies.PatientIntakeAccessLinkManage)]
    public sealed class PatientIntakeAccessLinksController : ControllerBase
    {
        private readonly IPatientIntakeAccessLinkCommandService _commandService;
        private readonly IPatientIntakeAccessLinkQueryService _queryService;

        public PatientIntakeAccessLinksController(
            IPatientIntakeAccessLinkCommandService commandService,
            IPatientIntakeAccessLinkQueryService queryService)
        {
            _commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            _queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<PatientIntakeAccessLinkSummaryDto>>> List(
            CancellationToken cancellationToken = default)
        {
            SetSensitiveResponseHeaders();
            var accessLinks = await _queryService.ListAsync(cancellationToken);
            return Ok(accessLinks);
        }

        [HttpPost]
        public async Task<ActionResult<IssuedPatientIntakeAccessLinkDto>> Issue(
            [FromBody] IssuePatientIntakeAccessLinkRequest? request,
            CancellationToken cancellationToken = default)
        {
            SetSensitiveResponseHeaders();

            try
            {
                var accessLink = await _commandService.IssueAsync(
                    request?.BranchId,
                    GetCorrelationId(),
                    cancellationToken);
                if (accessLink is null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Patient intake link context not found.",
                        Detail = "The selected operational context is not available."
                    });
                }

                return StatusCode(StatusCodes.Status201Created, accessLink);
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
                    Title = "Patient intake link conflict.",
                    Detail = exception.Message
                });
            }
        }

        [HttpDelete("{accessLinkId:guid}")]
        public async Task<IActionResult> Revoke(
            Guid accessLinkId,
            CancellationToken cancellationToken = default)
        {
            SetSensitiveResponseHeaders();

            try
            {
                var revoked = await _commandService.RevokeAsync(
                    accessLinkId,
                    GetCorrelationId(),
                    cancellationToken);
                return revoked ? NoContent() : NotFound();
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
                    Title = "Patient intake link conflict.",
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
            ModelState.AddModelError(nameof(PatientIntakeAccessLinksController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class IssuePatientIntakeAccessLinkRequest
        {
            public Guid? BranchId { get; set; }
        }
    }
}
