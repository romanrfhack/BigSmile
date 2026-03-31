using BigSmile.Infrastructure.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;

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
            // This is a placeholder implementation and must be replaced before production deployment.
            var tenantId = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var branchId = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();

            // Validate tenantId format (must be a valid GUID if provided)
            if (!string.IsNullOrEmpty(tenantId))
            {
                if (!Guid.TryParse(tenantId, out var tenantGuid))
                {
                    var logger = httpContext.RequestServices.GetRequiredService<ILogger<TenantResolutionMiddleware>>();
                    logger.LogWarning("Invalid tenant ID format in X-Tenant-Id header: {TenantId}", tenantId);
                    tenantId = null;
                }
            }

            if (!string.IsNullOrEmpty(branchId))
            {
                if (!Guid.TryParse(branchId, out var branchGuid))
                {
                    var logger = httpContext.RequestServices.GetRequiredService<ILogger<TenantResolutionMiddleware>>();
                    logger.LogWarning("Invalid branch ID format in X-Branch-Id header: {BranchId}", branchId);
                    branchId = null;
                }
            }

            // Log a warning if running in production and using header-based resolution
            var env = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
            if (!env.IsDevelopment() && (!string.IsNullOrEmpty(tenantId) || !string.IsNullOrEmpty(branchId)))
            {
                var logger = httpContext.RequestServices.GetRequiredService<ILogger<TenantResolutionMiddleware>>();
                logger.LogWarning("Header-based tenant resolution is being used in a non-development environment. This is insecure and should be replaced with a secure tenant resolution strategy (e.g., JWT claims, subdomain).");
            }

            if (!string.IsNullOrEmpty(tenantId))
                tenantContext.SetTenantId(tenantId);

            if (!string.IsNullOrEmpty(branchId))
                tenantContext.SetBranchId(branchId);

            await _next(httpContext);
        }
    }
}