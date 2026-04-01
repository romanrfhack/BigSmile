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
            var logger = httpContext.RequestServices.GetRequiredService<ILogger<TenantResolutionMiddleware>>();
            var env = httpContext.RequestServices.GetRequiredService<IHostEnvironment>();

            // 1. Try to resolve tenant from JWT claim (preferred)
            var user = httpContext.User;
            if (user != null)
            {
                var tenantClaim = user.Claims.FirstOrDefault(c => c.Type == "tenant_id");
                var branchClaim = user.Claims.FirstOrDefault(c => c.Type == "branch_id");
                
                if (tenantClaim != null)
                {
                    // Tenant resolved from JWT claim
                    tenantContext.SetTenantId(tenantClaim.Value);
                    logger.LogDebug("Tenant resolved from JWT claim: {TenantId}", tenantClaim.Value);
                    
                    if (branchClaim != null)
                    {
                        tenantContext.SetBranchId(branchClaim.Value);
                        logger.LogDebug("Branch resolved from JWT claim: {BranchId}", branchClaim.Value);
                    }
                    
                    await _next(httpContext);
                    return;
                }
            }

            // 2. No tenant claim present; check if we're in development environment
            if (!env.IsDevelopment())
            {
                // Non-development environment: header-based resolution is forbidden
                var tenantHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
                var branchHeader = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantHeader) || !string.IsNullOrEmpty(branchHeader))
                {
                    logger.LogError(
                        "Header-based tenant resolution is not allowed in non-development environments when JWT claim is missing. " +
                        "Request headers X-Tenant-Id and X-Branch-Id are ignored. " +
                        "Use a secure tenant resolution strategy (e.g., JWT claims, subdomain).");
                }
                // Tenant context remains empty; downstream authorization will fail.
                await _next(httpContext);
                return;
            }

            // 3. Development environment: allow header-based resolution (with warnings if malformed)
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
            {
                tenantContext.SetTenantId(tenantIdDev);
                logger.LogDebug("Tenant resolved from header (development only): {TenantId}", tenantIdDev);
            }

            if (!string.IsNullOrEmpty(branchIdDev))
            {
                tenantContext.SetBranchId(branchIdDev);
                logger.LogDebug("Branch resolved from header (development only): {BranchId}", branchIdDev);
            }

            await _next(httpContext);
        }
    }
}