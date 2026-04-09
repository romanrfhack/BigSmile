using BigSmile.Application.Authorization;
using BigSmile.Infrastructure.Context;
using BigSmile.SharedKernel.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

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

            var user = httpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = FindFirstValue(user, ClaimTypes.NameIdentifier) ?? FindFirstValue(user, ClaimTypes.Name);
                var tenantId = FindFirstValue(user, BigSmileClaimTypes.TenantId);
                var branchId = FindFirstValue(user, BigSmileClaimTypes.BranchId);
                var scope = FindFirstValue(user, BigSmileClaimTypes.Scope).ToAccessScope();

                if (scope == AccessScope.Anonymous)
                {
                    scope = ResolveFallbackScope(user, tenantId, branchId);
                }

                tenantContext.SetRequestContext(
                    userId,
                    scope,
                    isAuthenticated: true,
                    tenantId,
                    branchId);

                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    logger.LogDebug("Tenant resolved from claims: {TenantId}", tenantId);
                }

                if (!string.IsNullOrWhiteSpace(branchId))
                {
                    logger.LogDebug("Branch resolved from claims: {BranchId}", branchId);
                }

                await _next(httpContext);
                return;
            }

            tenantContext.SetRequestContext(
                userId: null,
                accessScope: AccessScope.Anonymous,
                isAuthenticated: false);

            if (!env.IsDevelopment())
            {
                var tenantHeader = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
                var branchHeader = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();
                if (!string.IsNullOrEmpty(tenantHeader) || !string.IsNullOrEmpty(branchHeader))
                {
                    logger.LogError(
                        "Header-based tenant resolution is not allowed in non-development environments. " +
                        "Request headers X-Tenant-Id and X-Branch-Id are ignored.");
                }

                await _next(httpContext);
                return;
            }

            var tenantIdDev = httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            var branchIdDev = httpContext.Request.Headers["X-Branch-Id"].FirstOrDefault();

            if (!string.IsNullOrEmpty(tenantIdDev))
            {
                if (!Guid.TryParse(tenantIdDev, out _))
                {
                    logger.LogWarning("Invalid tenant ID format in X-Tenant-Id header: {TenantId}", tenantIdDev);
                    tenantIdDev = null;
                }
            }

            if (!string.IsNullOrEmpty(branchIdDev))
            {
                if (!Guid.TryParse(branchIdDev, out _))
                {
                    logger.LogWarning("Invalid branch ID format in X-Branch-Id header: {BranchId}", branchIdDev);
                    branchIdDev = null;
                }
            }

            if (!string.IsNullOrEmpty(tenantIdDev))
            {
                tenantContext.SetTenantId(tenantIdDev);
                tenantContext.SetAccessScope(string.IsNullOrEmpty(branchIdDev) ? AccessScope.Tenant : AccessScope.Branch);
                logger.LogDebug("Tenant resolved from header (development only): {TenantId}", tenantIdDev);
            }

            if (!string.IsNullOrEmpty(branchIdDev))
            {
                tenantContext.SetBranchId(branchIdDev);
                tenantContext.SetAccessScope(AccessScope.Branch);
                logger.LogDebug("Branch resolved from header (development only): {BranchId}", branchIdDev);
            }

            await _next(httpContext);
        }

        private static AccessScope ResolveFallbackScope(ClaimsPrincipal user, string? tenantId, string? branchId)
        {
            if (user.IsInRole(SystemRoles.PlatformAdmin) || string.Equals(
                    FindFirstValue(user, BigSmileClaimTypes.Role),
                    SystemRoles.PlatformAdmin,
                    StringComparison.OrdinalIgnoreCase))
            {
                return AccessScope.Platform;
            }

            if (!string.IsNullOrWhiteSpace(branchId))
            {
                return AccessScope.Branch;
            }

            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return AccessScope.Tenant;
            }

            return AccessScope.Anonymous;
        }

        private static string? FindFirstValue(ClaimsPrincipal user, string claimType)
        {
            return user.FindFirst(claimType)?.Value;
        }
    }
}
