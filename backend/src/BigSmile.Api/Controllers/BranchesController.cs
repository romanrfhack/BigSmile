using BigSmile.Api.Authorization;
using BigSmile.Application.Features.Branches.Dtos;
using BigSmile.Application.Features.Branches.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchQueryService _branchQueryService;

        public BranchesController(IBranchQueryService branchQueryService)
        {
            _branchQueryService = branchQueryService ?? throw new ArgumentNullException(nameof(branchQueryService));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = AuthorizationPolicies.BranchRead)]
        public async Task<ActionResult<BranchDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var branch = await _branchQueryService.GetBranchByIdAsync(id, cancellationToken);
            if (branch == null)
            {
                return NotFound();
            }

            return Ok(branch);
        }
    }
}
