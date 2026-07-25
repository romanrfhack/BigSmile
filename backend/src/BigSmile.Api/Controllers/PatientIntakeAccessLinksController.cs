using System.ComponentModel.DataAnnotations;
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
    [Authorize(Policy = AuthorizationPolicies.PatientPortalIntakeManage)]
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
            [FromQuery] bool includeResolved = false,
            [FromQuery][Range(1, PatientIntakeAccessLinkQueryService.MaximumReturnedLinks)] int take = 50,
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            try
            {
                var links = await _queryService.ListAsync(
                    includeResolved,
                    take,
                    cancellationToken);
                return Ok(links);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildConflictProblem(exception.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<IssuedPatientIntakeAccessLinkDto>> Issue(
            [FromBody] IssuePatientIntakeAccessLinkRequest? request,
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            try
            {
                var result = await _commandService.IssueAsync(
                    request?.BranchId,
                    GetCorrelationId(),
                    cancellationToken);

                return result.Failure switch
                {
                    PatientIntakeAccessLinkIssueFailure.None => StatusCode(
                        StatusCodes.Status201Created,
                        result.Link),
                    PatientIntakeAccessLinkIssueFailure.BranchUnavailable => NotFound(new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Waiting-room branch not available.",
                        Detail = "The selected branch is not available for this tenant."
                    }),
                    _ => BuildConflictProblem(
                        "The waiting-room access link could not be issued because its state changed.")
                };
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildConflictProblem(exception.Message);
            }
        }

        [HttpDelete("{linkId:guid}")]
        public async Task<IActionResult> Revoke(
            Guid linkId,
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();

            try
            {
                var result = await _commandService.RevokeAsync(
                    linkId,
                    GetCorrelationId(),
                    cancellationToken);

                return result.Failure switch
                {
                    PatientIntakeAccessLinkRevokeFailure.None => NoContent(),
                    PatientIntakeAccessLinkRevokeFailure.Missing => NotFound(),
                    PatientIntakeAccessLinkRevokeFailure.NotActive => BuildConflictProblem(
                        "Only an active waiting-room access link can be revoked."),
                    _ => BuildConflictProblem(
                        "The waiting-room access link changed. Reload before trying again.")
                };
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildConflictProblem(exception.Message);
            }
        }

        private void SetNoStoreHeaders()
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

        private ObjectResult BuildConflictProblem(string detail)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Waiting-room access link conflict.",
                Detail = detail
            });
        }

        public sealed class IssuePatientIntakeAccessLinkRequest
        {
            public Guid? BranchId { get; set; }
        }
    }
}
