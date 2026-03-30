using BigSmile.Application.Features.Tenants.Dtos;
using BigSmile.Application.Features.Tenants.Queries;
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

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TenantDto>> GetById(Guid id)
        {
            var tenant = await _tenantQueryService.GetTenantByIdAsync(id);
            if (tenant == null)
                return NotFound();

            return Ok(tenant);
        }
    }
}