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
            // Get environment and logger
            var env = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<TenantResolutionMiddleware>>();

            // Header-based tenant resolution is allowed only in Development environment
            if (!env.IsDevelopment())
            {
                // Check if headers are present (even if we will ignore them)
                var tenantId = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
                var branchId = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantId) || !string.IsNullOrEmpty(branchId))
                {
                    logger.LogError(
                        "Header-based tenant resolution is not allowed in non-development environments. " +
                        "Request headers X-Tenant-Id and X-Branch-Id are ignored. " +
                        "Use a secure tenant resolution strategy (e.g., JWT claims, subdomain).");
                }
                // Do NOT set tenant/branch from headers in non-development
                // The tenant context will remain empty, which will cause downstream authorization to fail.
                // This is intentional: it forces the adoption of a proper tenant resolution strategy.
                await _next(httpContext);
                return;
            }

            // Development environment: allow header-based resolution (with warnings if malformed)
            var tenantIdDev = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var branchIdDev = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();

            // Validate tenantId format (must be a valid GUID if provided)
            if (!string.IsNullOrEmpty(tenantIdDev))
            {
                if (!Guid.TryParse(tenantIdDev, out var tenantGuid))
                {
                    logger.LogWarning("Invalid tenant ID format in X-Tenant-Id header: {TenantId}", tenantIdDev);
                    tenantIdDev = null;
                }
            }

            if (!string.IsNullOrEmpty(branchIdDev))
            {
                if (!Guid.TryParse(branchIdDev, out var branchGuid))
                {
                    logger.LogWarning("Invalid branch ID format in X-Branch-Id header: {BranchId}", branchIdDev);
                    branchIdDev = null;
                }
            }

            if (!string.IsNullOrEmpty(tenantIdDev))
                tenantContext.SetTenantId(tenantIdDev);

            if (!string.IsNullOrEmpty(branchIdDev))
                tenantContext.SetBranchId(branchIdDev);

            await _next(httpContext);
        }
    }
}