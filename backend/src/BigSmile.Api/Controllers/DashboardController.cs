using BigSmile.Api.Authorization;
using BigSmile.Application.Features.Dashboard.Dtos;
using BigSmile.Application.Features.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public sealed class DashboardController : ControllerBase
    {
        private readonly IDashboardSummaryQueryService _dashboardSummaryQueryService;

        public DashboardController(IDashboardSummaryQueryService dashboardSummaryQueryService)
        {
            _dashboardSummaryQueryService = dashboardSummaryQueryService ?? throw new ArgumentNullException(nameof(dashboardSummaryQueryService));
        }

        [HttpGet("summary")]
        [Authorize(Policy = AuthorizationPolicies.DashboardRead)]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = await _dashboardSummaryQueryService.GetSummaryAsync(cancellationToken);
                return Ok(summary);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(DashboardController), message);
            return ValidationProblem(ModelState);
        }
    }
}
