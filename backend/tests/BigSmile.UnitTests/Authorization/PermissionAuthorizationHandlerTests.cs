using BigSmile.Api.Authorization;
using BigSmile.Application.Authorization;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;

namespace BigSmile.UnitTests.Authorization
{
    public class PermissionAuthorizationHandlerTests
    {
        private readonly TenantContext _tenantContext = new();
        private readonly Mock<IBranchRepository> _branchRepository = new();
        private readonly Mock<IUserTenantMembershipRepository> _membershipRepository = new();
        private readonly DefaultHttpContext _httpContext = new();
        private readonly PermissionAuthorizationHandler _handler;
        private readonly RolePermissionCatalog _rolePermissionCatalog = new();

        public PermissionAuthorizationHandlerTests()
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = _httpContext
            };

            _handler = new PermissionAuthorizationHandler(
                accessor,
                _tenantContext,
                _branchRepository.Object,
                _membershipRepository.Object,
                Mock.Of<ILogger<PermissionAuthorizationHandler>>());
        }

        [Fact]
        public async Task TenantAccessRequirement_Succeeds_ForCurrentTenant()
        {
            var userId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Tenant, isAuthenticated: true, tenantId.ToString());
            _httpContext.Request.RouteValues["id"] = tenantId.ToString();

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.TenantRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new TenantAccessRequirement(Permissions.TenantRead, "id", allowPlatformOverride: true) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task TenantAccessRequirement_Fails_ForCrossTenantRequest()
        {
            var userId = Guid.NewGuid();
            var currentTenantId = Guid.NewGuid();
            var requestedTenantId = Guid.NewGuid();
            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Tenant, isAuthenticated: true, currentTenantId.ToString());
            _httpContext.Request.RouteValues["id"] = requestedTenantId.ToString();

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.TenantRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new TenantAccessRequirement(Permissions.TenantRead, "id", allowPlatformOverride: true) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task TenantAccessRequirement_AllowsExplicitPlatformOverride_WhenConfigured()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Platform, isAuthenticated: true);
            _httpContext.Request.RouteValues["id"] = Guid.NewGuid().ToString();

            var user = CreatePrincipal(userId, SystemRoles.PlatformAdmin, Permissions.TenantRead, Permissions.PlatformTenantsRead, Permissions.BranchReadAny);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new TenantAccessRequirement(Permissions.TenantRead, "id", allowPlatformOverride: true) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.True(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task TenantAccessRequirement_BlocksPlatformOverride_WhenRequirementDoesNotAllowIt()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Platform, isAuthenticated: true);
            _httpContext.Request.RouteValues["id"] = Guid.NewGuid().ToString();

            var user = CreatePrincipal(userId, SystemRoles.PlatformAdmin, Permissions.TenantRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new TenantAccessRequirement(Permissions.TenantRead, "id", allowPlatformOverride: false) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task PermissionRequirement_Succeeds_ForCurrentTenantRead_WhenTenantContextIsResolved()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.TenantRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[]
                {
                    new PermissionRequirement(Permissions.TenantRead, requireResolvedTenantContext: true)
                },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task PermissionRequirement_Fails_ForCurrentTenantRead_WhenTenantContextIsMissing()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Platform,
                isAuthenticated: true);

            var user = CreatePrincipal(userId, SystemRoles.PlatformAdmin, Permissions.TenantRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[]
                {
                    new PermissionRequirement(Permissions.TenantRead, requireResolvedTenantContext: true)
                },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task BranchAccessRequirement_Succeeds_ForAssignedTenantUser()
        {
            var userId = Guid.NewGuid();
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("North");
            var membership = CreateMembershipForUser(userId, tenant, SystemRoles.TenantUser, branch);

            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Tenant, isAuthenticated: true, tenant.Id.ToString());
            _httpContext.Request.RouteValues["id"] = branch.Id.ToString();

            _branchRepository
                .Setup(repository => repository.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(branch);
            _membershipRepository
                .Setup(repository => repository.GetByUserAndTenantAsync(userId, tenant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(membership);

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.BranchReadAssigned);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new BranchAccessRequirement("id", allowPlatformOverride: true) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task BranchAccessRequirement_Fails_ForUnassignedTenantUser()
        {
            var userId = Guid.NewGuid();
            var tenant = new Tenant("Tenant A", "tenant-a");
            var branch = tenant.AddBranch("North");
            var membership = CreateMembershipForUser(userId, tenant, SystemRoles.TenantUser);

            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Tenant, isAuthenticated: true, tenant.Id.ToString());
            _httpContext.Request.RouteValues["id"] = branch.Id.ToString();

            _branchRepository
                .Setup(repository => repository.GetByIdAsync(branch.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(branch);
            _membershipRepository
                .Setup(repository => repository.GetByUserAndTenantAsync(userId, tenant.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(membership);

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.BranchReadAssigned);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new BranchAccessRequirement("id", allowPlatformOverride: true) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_RequiresPlatformScope_ForPlatformOnlyPolicy()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(userId.ToString(), BigSmile.SharedKernel.Authorization.AccessScope.Tenant, isAuthenticated: true, Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantAdmin, Permissions.PlatformTenantsRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[]
                {
                    new PermissionRequirement(Permissions.PlatformTenantsRead, requirePlatformScope: true, enablePlatformOverride: true)
                },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Succeeds_ForGrantedPatientPermission()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.PatientRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.PatientRead) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Fails_WhenPatientPermissionIsMissing()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.TenantRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.PatientWrite) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Succeeds_ForGrantedSchedulingPermission()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.SchedulingWrite);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.SchedulingWrite) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Fails_WhenSchedulingPermissionIsMissing()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.PatientRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.SchedulingRead) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Succeeds_ForGrantedClinicalPermission()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantAdmin, Permissions.ClinicalWrite);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.ClinicalWrite) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task PermissionRequirement_Fails_WhenClinicalPermissionIsMissing()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.PatientRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.ClinicalRead) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Succeeds_ForGrantedOdontogramPermission()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantAdmin, Permissions.OdontogramWrite);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.OdontogramWrite) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_Fails_WhenOdontogramPermissionIsMissing()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantUser, Permissions.PatientRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[] { new PermissionRequirement(Permissions.OdontogramRead) },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
        }

        [Fact]
        public async Task PermissionRequirement_AllowsExplicitPlatformOverride_ForDocumentPolicy()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Platform,
                isAuthenticated: true);
            _httpContext.Request.RouteValues["patientId"] = Guid.NewGuid().ToString();

            var user = CreatePrincipal(userId, SystemRoles.PlatformAdmin, Permissions.DocumentRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[]
                {
                    new PermissionRequirement(
                        Permissions.DocumentRead,
                        enablePlatformOverride: true,
                        platformOverrideRouteValueKey: "patientId")
                },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.True(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task PermissionRequirement_Succeeds_ForDashboardPermissionWithResolvedTenantContext()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Tenant,
                isAuthenticated: true,
                Guid.NewGuid().ToString());

            var user = CreatePrincipal(userId, SystemRoles.TenantAdmin, Permissions.DashboardRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[]
                {
                    new PermissionRequirement(
                        Permissions.DashboardRead,
                        requireResolvedTenantContext: true)
                },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.True(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public async Task PermissionRequirement_Fails_ForDashboardPermissionInPlatformScope()
        {
            var userId = Guid.NewGuid();
            _tenantContext.SetRequestContext(
                userId.ToString(),
                BigSmile.SharedKernel.Authorization.AccessScope.Platform,
                isAuthenticated: true);

            var user = CreatePrincipal(userId, SystemRoles.PlatformAdmin, Permissions.DashboardRead);
            var context = new AuthorizationHandlerContext(
                new IAuthorizationRequirement[]
                {
                    new PermissionRequirement(
                        Permissions.DashboardRead,
                        requireResolvedTenantContext: true)
                },
                user,
                resource: null);

            await _handler.HandleAsync(context);

            Assert.False(context.HasSucceeded);
            Assert.False(_tenantContext.HasPlatformOverride());
        }

        [Fact]
        public void RolePermissionCatalog_DoesNotGrantClinicalPermissions_ToTenantUser()
        {
            var permissions = _rolePermissionCatalog.GetPermissions(SystemRoles.TenantUser);

            Assert.DoesNotContain(Permissions.ClinicalRead, permissions);
            Assert.DoesNotContain(Permissions.ClinicalWrite, permissions);
        }

        [Fact]
        public void RolePermissionCatalog_DoesNotGrantOdontogramPermissions_ToTenantUser()
        {
            var permissions = _rolePermissionCatalog.GetPermissions(SystemRoles.TenantUser);

            Assert.DoesNotContain(Permissions.OdontogramRead, permissions);
            Assert.DoesNotContain(Permissions.OdontogramWrite, permissions);
        }

        [Fact]
        public void RolePermissionCatalog_DoesNotGrantDocumentPermissions_ToTenantUser()
        {
            var permissions = _rolePermissionCatalog.GetPermissions(SystemRoles.TenantUser);

            Assert.DoesNotContain(Permissions.DocumentRead, permissions);
            Assert.DoesNotContain(Permissions.DocumentWrite, permissions);
        }

        [Fact]
        public void RolePermissionCatalog_DoesNotGrantDashboardPermission_ToTenantUser()
        {
            var permissions = _rolePermissionCatalog.GetPermissions(SystemRoles.TenantUser);

            Assert.DoesNotContain(Permissions.DashboardRead, permissions);
        }

        private static ClaimsPrincipal CreatePrincipal(Guid userId, string role, params string[] permissions)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString()),
                new(ClaimTypes.Role, role)
            };

            claims.AddRange(permissions.Select(permission => new Claim(BigSmile.SharedKernel.Authorization.BigSmileClaimTypes.Permission, permission)));

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        }

        private static UserTenantMembership CreateMembershipForUser(Guid userId, Tenant tenant, string roleName, Branch? assignedBranch = null)
        {
            var user = new User($"user-{userId}@test.local", "hashed-password", "Test User");
            var role = new Role(roleName);
            var membership = user.AddTenantMembership(tenant, role);

            if (assignedBranch != null)
            {
                membership.AssignToBranch(assignedBranch);
            }

            return membership;
        }
    }
}
