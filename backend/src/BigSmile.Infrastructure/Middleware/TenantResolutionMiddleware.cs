using BigSmile.Infrastructure.Context;
using Microsoft.AspNetCore.Http;

namespace BigSmile.Infrastructure.Middleware
{
    public class TenantResolutionMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantResolutionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, TenantContext tenantContext)
        {
            // For now, read from headers (example: X-Tenant-Id, X-Branch-Id)
            // In production, this could be derived from subdomain, JWT claim, etc.
            var tenantId = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var branchId = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(tenantId))
                tenantContext.SetTenantId(tenantId);

            if (!string.IsNullOrEmpty(branchId))
                tenantContext.SetBranchId(branchId);

            await _next(httpContext);
        }
    }
}