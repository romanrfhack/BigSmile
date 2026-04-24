using BigSmile.Application.Authorization;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Infrastructure.Context;
using BigSmile.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BigSmile.Api.Authorization
{
    public sealed class PermissionAuthorizationHandler : IAuthorizationHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenantContext _tenantContext;
        private readonly IBranchRepository _branchRepository;
        private readonly IUserTenantMembershipRepository _membershipRepository;
        private readonly ILogger<PermissionAuthorizationHandler> _logger;

        public PermissionAuthorizationHandler(
            IHttpContextAccessor httpContextAccessor,
            TenantContext tenantContext,
            IBranchRepository branchRepository,
            IUserTenantMembershipRepository membershipRepository,
            ILogger<PermissionAuthorizationHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantContext = tenantContext;
            _branchRepository = branchRepository;
            _membershipRepository = membershipRepository;
            _logger = logger;
        }

        public async Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToArray())
            {
                switch (requirement)
                {
                    case PermissionRequirement permissionRequirement:
                        HandlePermissionRequirement(context, permissionRequirement);
                        break;
                    case TenantAccessRequirement tenantAccessRequirement:
                        await HandleTenantAccessRequirementAsync(context, tenantAccessRequirement);
                        break;
                    case BranchAccessRequirement branchAccessRequirement:
                        await HandleBranchAccessRequirementAsync(context, branchAccessRequirement);
                        break;
                }
            }
        }

        private void HandlePermissionRequirement(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (!HasPermission(context.User, requirement.Permission))
            {
                return;
            }

            if (requirement.RequirePlatformScope && _tenantContext.GetAccessScope() != AccessScope.Platform)
            {
                return;
            }

            if (requirement.EnablePlatformOverride && _tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                EnablePlatformOverride(
                    context.User,
                    requirement.Permission,
                    ResolvePlatformOverrideResourceId(requirement.PlatformOverrideRouteValueKey));
            }

            context.Succeed(requirement);
        }

        private async Task HandleTenantAccessRequirementAsync(AuthorizationHandlerContext context, TenantAccessRequirement requirement)
        {
            if (!HasPermission(context.User, requirement.Permission))
            {
                return;
            }

            var targetTenantId = GetRouteGuid(requirement.RouteValueKey);
            if (!targetTenantId.HasValue)
            {
                return;
            }

            if (_tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                if (!requirement.AllowPlatformOverride)
                {
                    return;
                }

                EnablePlatformOverride(context.User, requirement.Permission, targetTenantId.Value);
                context.Succeed(requirement);
                return;
            }

            if (Guid.TryParse(_tenantContext.GetTenantId(), out var currentTenantId) && currentTenantId == targetTenantId.Value)
            {
                context.Succeed(requirement);
            }
        }

        private async Task HandleBranchAccessRequirementAsync(AuthorizationHandlerContext context, BranchAccessRequirement requirement)
        {
            var targetBranchId = GetRouteGuid(requirement.RouteValueKey);
            if (!targetBranchId.HasValue)
            {
                return;
            }

            if (_tenantContext.GetAccessScope() == AccessScope.Platform)
            {
                if (!requirement.AllowPlatformOverride ||
                    (!HasPermission(context.User, Permissions.BranchReadAny) &&
                     !HasPermission(context.User, Permissions.BranchReadAssigned)))
                {
                    return;
                }

                EnablePlatformOverride(context.User, Permissions.BranchReadAny, targetBranchId.Value);
                context.Succeed(requirement);
                return;
            }

            if (!Guid.TryParse(_tenantContext.GetTenantId(), out var currentTenantId) ||
                !Guid.TryParse(_tenantContext.GetUserId(), out var currentUserId))
            {
                return;
            }

            var branch = await _branchRepository.GetByIdAsync(targetBranchId.Value);
            if (branch == null || branch.TenantId != currentTenantId)
            {
                return;
            }

            if (HasPermission(context.User, Permissions.BranchReadAny))
            {
                context.Succeed(requirement);
                return;
            }

            if (!HasPermission(context.User, Permissions.BranchReadAssigned))
            {
                return;
            }

            var membership = await _membershipRepository.GetByUserAndTenantAsync(currentUserId, currentTenantId);
            if (membership == null || !membership.IsActive)
            {
                return;
            }

            if (membership.BranchAssignments.Any(assignment => assignment.IsActive && assignment.BranchId == branch.Id))
            {
                context.Succeed(requirement);
            }
        }

        private Guid? GetRouteGuid(string routeValueKey)
        {
            var routeValue = _httpContextAccessor.HttpContext?.Request.RouteValues[routeValueKey]?.ToString();
            return Guid.TryParse(routeValue, out var parsed) ? parsed : null;
        }

        private Guid? ResolvePlatformOverrideResourceId(string? routeValueKey)
        {
            if (string.IsNullOrWhiteSpace(routeValueKey))
            {
                return null;
            }

            return GetRouteGuid(routeValueKey);
        }

        private static bool HasPermission(ClaimsPrincipal user, string permission)
        {
            return user.Claims.Any(claim =>
                claim.Type == BigSmileClaimTypes.Permission &&
                string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
        }

        private void EnablePlatformOverride(ClaimsPrincipal user, string permission, Guid? resourceId)
        {
            if (_tenantContext.HasPlatformOverride())
            {
                return;
            }

            _tenantContext.EnablePlatformOverride();

            var actorUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? "unknown";

            _logger.LogInformation(
                "Platform override enabled for actor {ActorUserId} and permission {Permission} on resource {ResourceId}.",
                actorUserId,
                permission,
                resourceId?.ToString() ?? "n/a");
        }
    }
}
