using BigSmile.Api.Authorization;
using BigSmile.Application.Authorization;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BigSmile.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTenantMembershipRepository _membershipRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRolePermissionCatalog _permissionCatalog;
        private readonly ITenantContext _tenantContext;

        public AuthController(
            IUserRepository userRepository,
            IUserTenantMembershipRepository membershipRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenService jwtTokenService,
            IRolePermissionCatalog permissionCatalog,
            ITenantContext tenantContext)
        {
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
            _permissionCatalog = permissionCatalog;
            _tenantContext = tenantContext;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
                return Unauthorized("Invalid email or password.");

            // Verify password
            if (!_passwordHasher.VerifyPassword(user.HashedPassword, request.Password))
                return Unauthorized("Invalid email or password.");

            var memberships = await _membershipRepository.GetByUserIdAsync(user.Id);
            var membership = SelectMembership(memberships);
            if (membership == null)
                return Unauthorized("User is not a member of any tenant.");

            var accessScope = DetermineAccessScope(membership);
            var permissions = _permissionCatalog.GetPermissions(membership.Role.Name);
            var currentBranch = ResolveCurrentBranch(accessScope, membership);
            var token = _jwtTokenService.GenerateToken(
                user,
                new AuthTokenDescriptor(
                    membership.Role.Name,
                    accessScope,
                    permissions,
                    membership.Role.Name.Equals(SystemRoles.PlatformAdmin, StringComparison.OrdinalIgnoreCase)
                        ? null
                        : membership.Tenant,
                    currentBranch));

            return new LoginResponse
            {
                Token = token,
                Current = BuildCurrentUserResponse(user, membership, accessScope, permissions, currentBranch)
            };
        }

        [Authorize(Policy = AuthorizationPolicies.SelfRead)]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(currentUserId, out var userId))
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            var roleName = User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue(BigSmileClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return Unauthorized();
            }

            var permissions = User.Claims
                .Where(claim => claim.Type == BigSmileClaimTypes.Permission)
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (permissions.Length == 0)
            {
                permissions = _permissionCatalog.GetPermissions(roleName).ToArray();
            }

            var accessScope = _tenantContext.GetAccessScope();
            if (accessScope == AccessScope.Platform)
            {
                return Ok(new CurrentUserResponse
                {
                    User = MapUser(user),
                    Role = roleName,
                    Scope = accessScope.ToClaimValue(),
                    Permissions = permissions
                });
            }

            var currentTenantId = _tenantContext.GetTenantId();
            if (!Guid.TryParse(currentTenantId, out var tenantId))
            {
                return Forbid();
            }

            var membership = await _membershipRepository.GetByUserAndTenantAsync(user.Id, tenantId);
            if (membership == null || !membership.IsActive)
            {
                return Forbid();
            }

            var currentBranch = ResolveCurrentBranch(accessScope, membership);
            return Ok(BuildCurrentUserResponse(user, membership, accessScope, permissions, currentBranch));
        }

        #region DTOs

        public class LoginRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, MinLength(6)]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
            public CurrentUserResponse Current { get; set; } = null!;
        }

        public class UserDto
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string? DisplayName { get; set; }
        }

        public class TenantDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class BranchDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class CurrentUserResponse
        {
            public UserDto User { get; set; } = null!;
            public TenantDto? Tenant { get; set; }
            public BranchDto? CurrentBranch { get; set; }
            public IReadOnlyCollection<BranchDto> Branches { get; set; } = Array.Empty<BranchDto>();
            public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
            public string Role { get; set; } = string.Empty;
            public string Scope { get; set; } = AccessScope.Anonymous.ToClaimValue();
        }

        #endregion

        private static UserTenantMembership? SelectMembership(IReadOnlyList<UserTenantMembership> memberships)
        {
            return memberships
                .Where(membership => membership.IsActive)
                .OrderByDescending(membership => membership.Role.Name.Equals(SystemRoles.PlatformAdmin, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(membership => membership.Role.Name.Equals(SystemRoles.TenantAdmin, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
        }

        private static AccessScope DetermineAccessScope(UserTenantMembership membership)
        {
            if (membership.Role.Name.Equals(SystemRoles.PlatformAdmin, StringComparison.OrdinalIgnoreCase))
            {
                return AccessScope.Platform;
            }

            var activeBranchCount = membership.BranchAssignments.Count(assignment => assignment.IsActive);
            return activeBranchCount == 1 ? AccessScope.Branch : AccessScope.Tenant;
        }

        private CurrentUserResponse BuildCurrentUserResponse(
            User user,
            UserTenantMembership membership,
            AccessScope accessScope,
            IReadOnlyCollection<string> permissions,
            Branch? currentBranch)
        {
            var activeBranches = membership.BranchAssignments
                .Where(assignment => assignment.IsActive)
                .Select(assignment => assignment.Branch)
                .DistinctBy(branch => branch.Id)
                .Select(branch => new BranchDto
                {
                    Id = branch.Id,
                    Name = branch.Name
                })
                .ToArray();

            return new CurrentUserResponse
            {
                User = MapUser(user),
                Tenant = membership.Role.Name.Equals(SystemRoles.PlatformAdmin, StringComparison.OrdinalIgnoreCase)
                    ? null
                    : new TenantDto
                    {
                        Id = membership.Tenant.Id,
                        Name = membership.Tenant.Name
                    },
                CurrentBranch = currentBranch == null
                    ? null
                    : new BranchDto
                    {
                        Id = currentBranch.Id,
                        Name = currentBranch.Name
                    },
                Branches = activeBranches,
                Permissions = permissions
                    .OrderBy(permission => permission, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                Role = membership.Role.Name,
                Scope = accessScope.ToClaimValue()
            };
        }

        private Branch? ResolveCurrentBranch(AccessScope accessScope, UserTenantMembership membership)
        {
            if (accessScope != AccessScope.Branch)
            {
                return null;
            }

            if (Guid.TryParse(_tenantContext.GetBranchId(), out var currentBranchId))
            {
                return membership.BranchAssignments
                    .Where(assignment => assignment.IsActive)
                    .Select(assignment => assignment.Branch)
                    .FirstOrDefault(branch => branch.Id == currentBranchId);
            }

            return membership.BranchAssignments
                .Where(assignment => assignment.IsActive)
                .Select(assignment => assignment.Branch)
                .FirstOrDefault();
        }

        private static UserDto MapUser(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName
            };
        }
    }
}
