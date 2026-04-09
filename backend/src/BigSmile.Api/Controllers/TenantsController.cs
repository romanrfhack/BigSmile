using BigSmile.Application.Features.Tenants.Dtos;
using BigSmile.Application.Features.Tenants.Queries;
using BigSmile.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantQueryService _tenantQueryService;

        public TenantsController(ITenantQueryService tenantQueryService)
        {
            _tenantQueryService = tenantQueryService ?? throw new ArgumentNullException(nameof(tenantQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.PlatformTenantsRead)]
        public async Task<ActionResult<IReadOnlyList<TenantDto>>> GetAll(CancellationToken cancellationToken)
        {
            var tenants = await _tenantQueryService.GetAllTenantsAsync(cancellationToken);
            return Ok(tenants);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = AuthorizationPolicies.TenantRead)]
        public async Task<ActionResult<TenantDto>> GetById(Guid id, CancellationToken cancellationToken)
        {
            var tenant = await _tenantQueryService.GetTenantByIdAsync(id, cancellationToken);
            if (tenant == null)
                return NotFound();

            return Ok(tenant);
        }
    }
}
