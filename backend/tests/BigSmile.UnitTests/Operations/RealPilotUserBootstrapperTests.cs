using BigSmile.Application.Authorization;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Data;
using Microsoft.Extensions.Logging.Abstractions;

namespace BigSmile.UnitTests.Operations
{
    public class RealPilotUserBootstrapperTests
    {
        [Fact]
        public async Task BootstrapAsync_Disabled_DoesNotCreateUsersOrRequirePasswords()
        {
            var fixture = BootstrapFixture.Create();
            var options = new RealPilotUserBootstrapOptions { Enabled = false };

            var result = await fixture.Bootstrapper.BootstrapAsync(options);

            Assert.Equal(0, result.UsersCreated);
            Assert.Empty(fixture.Users.Users);
        }

        [Fact]
        public async Task BootstrapAsync_EnabledWithoutRequiredPasswords_FailsWithoutCreatingUsers()
        {
            var fixture = BootstrapFixture.Create();
            var options = new RealPilotUserBootstrapOptions
            {
                Enabled = true,
                RomanPassword = "roman-test-password",
                ViridianaPassword = "viridiana-test-password",
                UsuarioSucursalPassword = "usuario-sucursal-test-password"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                fixture.Bootstrapper.BootstrapAsync(options));

            Assert.Contains(RealPilotUserBootstrapOptions.JorgePasswordKey, ex.Message);
            Assert.DoesNotContain("roman-test-password", ex.Message);
            Assert.Empty(fixture.Users.Users);
            Assert.Empty(fixture.PasswordHasher.HashedPasswords);
        }

        [Fact]
        public async Task BootstrapAsync_CreatesPilotUsersMembershipsAndDefaultBranchIdempotently()
        {
            var fixture = BootstrapFixture.Create();
            var options = CreateEnabledOptions();

            var firstResult = await fixture.Bootstrapper.BootstrapAsync(options);
            var secondResult = await fixture.Bootstrapper.BootstrapAsync(options);

            Assert.Equal(4, firstResult.UsersCreated);
            Assert.Equal(4, firstResult.MembershipsCreated);
            Assert.True(firstResult.BranchCreated);
            Assert.Equal(1, firstResult.BranchAssignmentsCreated);

            Assert.Equal(0, secondResult.UsersCreated);
            Assert.Equal(4, secondResult.UsersExisting);
            Assert.Equal(0, secondResult.MembershipsCreated);
            Assert.Equal(4, secondResult.MembershipsExisting);
            Assert.False(secondResult.BranchCreated);
            Assert.Equal(0, secondResult.BranchAssignmentsCreated);
            Assert.Equal(1, secondResult.BranchAssignmentsExisting);

            Assert.Equal(4, fixture.Users.Users.Count);
            Assert.Equal(4, fixture.Memberships.Memberships.Count);
            Assert.Single(fixture.Branches.Branches);
            Assert.Single(fixture.BranchAssignments.Assignments);
            Assert.Equal("Sucursal Principal", fixture.Branches.Branches.Single().Name);
            Assert.Equal(0, fixture.Memberships.UpdateCalls);

            AssertMembership(
                fixture,
                "roman@bigsmile.com.mx",
                SystemRoles.PlatformAdmin,
                expectedBranchAssignment: false);
            AssertMembership(
                fixture,
                "viridiana@bigsmile.com.mx",
                SystemRoles.TenantAdmin,
                expectedBranchAssignment: false);
            AssertMembership(
                fixture,
                "jorge@bigsmile.com.mx",
                SystemRoles.TenantAdmin,
                expectedBranchAssignment: false);
            AssertMembership(
                fixture,
                "usuarioSucursal@bigsmile.com.mx",
                SystemRoles.TenantUser,
                expectedBranchAssignment: true);
        }

        [Fact]
        public async Task BootstrapAsync_UsesExistingActiveBranchForTenantUser()
        {
            var fixture = BootstrapFixture.Create();
            var existingBranch = fixture.DefaultTenant.AddBranch("Centro");
            fixture.Branches.Branches.Add(existingBranch);

            var result = await fixture.Bootstrapper.BootstrapAsync(CreateEnabledOptions());

            var tenantUserMembership = fixture.Memberships.Memberships.Single(
                membership => membership.User.Email == "usuarioSucursal@bigsmile.com.mx");
            var assignment = tenantUserMembership.BranchAssignments.Single();

            Assert.False(result.BranchCreated);
            Assert.Single(fixture.Branches.Branches);
            Assert.Equal(existingBranch.Id, assignment.BranchId);
        }

        [Fact]
        public async Task BootstrapAsync_ExistingActiveBranchAssignmentDoesNotUpdateMembership()
        {
            var fixture = BootstrapFixture.CreateWithExistingTenantUserAssignment(activeAssignment: true);

            var result = await fixture.Bootstrapper.BootstrapAsync(CreateEnabledOptions());

            Assert.Equal(1, result.BranchAssignmentsExisting);
            Assert.Equal(0, result.BranchAssignmentsCreated);
            Assert.Equal(0, result.BranchAssignmentsReactivated);
            Assert.Equal(0, fixture.Memberships.UpdateCalls);
            Assert.Equal(0, fixture.BranchAssignments.AddCalls);
            Assert.Equal(0, fixture.BranchAssignments.SaveChangesCalls);
        }

        [Fact]
        public async Task BootstrapAsync_ExistingInactiveBranchAssignmentIsReactivatedWithoutUpdatingMembership()
        {
            var fixture = BootstrapFixture.CreateWithExistingTenantUserAssignment(activeAssignment: false);
            var assignment = fixture.BranchAssignments.Assignments.Single();
            Assert.False(assignment.IsActive);
            Assert.NotNull(assignment.UnassignedAt);

            var result = await fixture.Bootstrapper.BootstrapAsync(CreateEnabledOptions());

            Assert.Equal(0, result.BranchAssignmentsExisting);
            Assert.Equal(0, result.BranchAssignmentsCreated);
            Assert.Equal(1, result.BranchAssignmentsReactivated);
            Assert.True(assignment.IsActive);
            Assert.Null(assignment.UnassignedAt);
            Assert.Equal(0, fixture.Memberships.UpdateCalls);
            Assert.Equal(0, fixture.BranchAssignments.AddCalls);
            Assert.Equal(1, fixture.BranchAssignments.SaveChangesCalls);
        }

        [Fact]
        public async Task BootstrapAsync_MissingBranchAssignmentIsCreatedWithoutUpdatingMembership()
        {
            var fixture = BootstrapFixture.CreateWithExistingTenantUserWithoutAssignment();

            var result = await fixture.Bootstrapper.BootstrapAsync(CreateEnabledOptions());

            var assignment = Assert.Single(fixture.BranchAssignments.Assignments);
            var tenantUserMembership = fixture.Memberships.Memberships.Single(
                membership => membership.User.Email == "usuarioSucursal@bigsmile.com.mx");

            Assert.Equal(1, result.BranchAssignmentsCreated);
            Assert.Equal(0, result.BranchAssignmentsExisting);
            Assert.Equal(0, result.BranchAssignmentsReactivated);
            Assert.Equal(tenantUserMembership.Id, assignment.MembershipId);
            Assert.Equal(fixture.Branches.Branches.Single().Id, assignment.BranchId);
            Assert.True(assignment.IsActive);
            Assert.Null(assignment.UnassignedAt);
            Assert.True(assignment.AssignedAt <= DateTime.UtcNow);
            Assert.Equal(0, fixture.Memberships.UpdateCalls);
            Assert.Equal(1, fixture.BranchAssignments.AddCalls);
        }

        [Fact]
        public async Task BootstrapAsync_ExistingMembershipWithWrongRoleFailsClearly()
        {
            var fixture = BootstrapFixture.Create();
            var roman = new User("roman@bigsmile.com.mx", "existing-hash", "Roman");
            fixture.Users.Users.Add(roman);
            fixture.Memberships.Memberships.Add(
                roman.AddTenantMembership(fixture.DefaultTenant, fixture.Roles.GetRequired(SystemRoles.TenantUser)));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                fixture.Bootstrapper.BootstrapAsync(CreateEnabledOptions()));

            Assert.Contains("roman@bigsmile.com.mx", ex.Message);
            Assert.Contains(SystemRoles.PlatformAdmin, ex.Message);
        }

        private static RealPilotUserBootstrapOptions CreateEnabledOptions()
        {
            return new RealPilotUserBootstrapOptions
            {
                Enabled = true,
                RomanPassword = "roman-test-password",
                ViridianaPassword = "viridiana-test-password",
                JorgePassword = "jorge-test-password",
                UsuarioSucursalPassword = "usuario-sucursal-test-password"
            };
        }

        private static void AssertMembership(
            BootstrapFixture fixture,
            string email,
            string roleName,
            bool expectedBranchAssignment)
        {
            var user = fixture.Users.Users.Single(candidate => candidate.Email == email);
            var membership = fixture.Memberships.Memberships.Single(candidate =>
                candidate.UserId == user.Id && candidate.TenantId == fixture.DefaultTenant.Id);

            Assert.True(membership.IsActive);
            Assert.Equal(roleName, membership.Role.Name);

            if (expectedBranchAssignment)
            {
                var assignment = Assert.Single(membership.BranchAssignments);
                Assert.True(assignment.IsActive);
                Assert.Equal(fixture.DefaultTenant.Id, assignment.Branch.TenantId);
            }
            else
            {
                Assert.Empty(membership.BranchAssignments);
            }
        }

        private sealed class BootstrapFixture
        {
            private BootstrapFixture(
                Tenant defaultTenant,
                InMemoryTenantRepository tenants,
                InMemoryBranchRepository branches,
                InMemoryUserRepository users,
                InMemoryMembershipRepository memberships,
                InMemoryBranchAssignmentStore branchAssignments,
                InMemoryRoleRepository roles,
                RecordingPasswordHasher passwordHasher,
                RealPilotUserBootstrapper bootstrapper)
            {
                DefaultTenant = defaultTenant;
                Tenants = tenants;
                Branches = branches;
                Users = users;
                Memberships = memberships;
                BranchAssignments = branchAssignments;
                Roles = roles;
                PasswordHasher = passwordHasher;
                Bootstrapper = bootstrapper;
            }

            public Tenant DefaultTenant { get; }
            public InMemoryTenantRepository Tenants { get; }
            public InMemoryBranchRepository Branches { get; }
            public InMemoryUserRepository Users { get; }
            public InMemoryMembershipRepository Memberships { get; }
            public InMemoryBranchAssignmentStore BranchAssignments { get; }
            public InMemoryRoleRepository Roles { get; }
            public RecordingPasswordHasher PasswordHasher { get; }
            public RealPilotUserBootstrapper Bootstrapper { get; }

            public static BootstrapFixture Create()
            {
                var defaultTenant = new Tenant("BigSmile Pilot", "default");
                var tenants = new InMemoryTenantRepository(defaultTenant);
                var branches = new InMemoryBranchRepository();
                var users = new InMemoryUserRepository();
                var memberships = new InMemoryMembershipRepository();
                var branchAssignments = new InMemoryBranchAssignmentStore();
                var roles = new InMemoryRoleRepository(
                    new Role(SystemRoles.PlatformAdmin, isSystem: true),
                    new Role(SystemRoles.TenantAdmin, isSystem: true),
                    new Role(SystemRoles.TenantUser, isSystem: true));
                var passwordHasher = new RecordingPasswordHasher();
                var bootstrapper = new RealPilotUserBootstrapper(
                    tenants,
                    branches,
                    users,
                    memberships,
                    branchAssignments,
                    roles,
                    passwordHasher,
                    NullLogger<RealPilotUserBootstrapper>.Instance);

                return new BootstrapFixture(
                    defaultTenant,
                    tenants,
                    branches,
                    users,
                    memberships,
                    branchAssignments,
                    roles,
                    passwordHasher,
                    bootstrapper);
            }

            public static BootstrapFixture CreateWithExistingTenantUserWithoutAssignment()
            {
                var fixture = Create();
                var branch = fixture.DefaultTenant.AddBranch("Centro");
                fixture.Branches.Branches.Add(branch);
                var tenantUser = new User("usuarioSucursal@bigsmile.com.mx", "existing-hash", "Usuario Sucursal");
                fixture.Users.Users.Add(tenantUser);
                fixture.Memberships.Memberships.Add(
                    tenantUser.AddTenantMembership(fixture.DefaultTenant, fixture.Roles.GetRequired(SystemRoles.TenantUser)));

                return fixture;
            }

            public static BootstrapFixture CreateWithExistingTenantUserAssignment(bool activeAssignment)
            {
                var fixture = CreateWithExistingTenantUserWithoutAssignment();
                var branch = fixture.Branches.Branches.Single();
                var membership = fixture.Memberships.Memberships.Single(
                    candidate => candidate.User.Email == "usuarioSucursal@bigsmile.com.mx");
                var assignment = membership.AssignToBranch(branch);

                if (!activeAssignment)
                {
                    assignment.Deactivate();
                }

                fixture.BranchAssignments.Assignments.Add(assignment);
                return fixture;
            }
        }

        private sealed class InMemoryTenantRepository : ITenantRepository
        {
            public InMemoryTenantRepository(params Tenant[] tenants)
            {
                Tenants.AddRange(tenants);
            }

            public List<Tenant> Tenants { get; } = new();

            public Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Tenants.FirstOrDefault(tenant => tenant.Id == id));
            }

            public Task<Tenant?> GetBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Tenants.FirstOrDefault(tenant =>
                    tenant.Subdomain != null &&
                    tenant.Subdomain.Equals(subdomain, StringComparison.OrdinalIgnoreCase)));
            }

            public Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<Tenant>>(Tenants.ToArray());
            }

            public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
            {
                Tenants.Add(tenant);
                return Task.CompletedTask;
            }

            public Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                Tenants.RemoveAll(tenant => tenant.Id == id);
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryBranchRepository : IBranchRepository
        {
            public List<Branch> Branches { get; } = new();

            public Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Branches.FirstOrDefault(branch => branch.Id == id));
            }

            public Task<IReadOnlyList<Branch>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<Branch>>(
                    Branches.Where(branch => branch.TenantId == tenantId).ToArray());
            }

            public Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
            {
                if (Branches.All(existing => existing.Id != branch.Id))
                {
                    Branches.Add(branch);
                }

                return Task.CompletedTask;
            }

            public Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            {
                Branches.RemoveAll(branch => branch.Id == id);
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryUserRepository : IUserRepository
        {
            public List<User> Users { get; } = new();

            public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Users.FirstOrDefault(user => user.Id == id));
            }

            public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Users.FirstOrDefault(user =>
                    user.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
            }

            public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
            {
                Users.Add(user);
                return Task.FromResult(user);
            }

            public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryMembershipRepository : IUserTenantMembershipRepository
        {
            public List<UserTenantMembership> Memberships { get; } = new();
            public int UpdateCalls { get; private set; }

            public Task<UserTenantMembership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Memberships.FirstOrDefault(membership => membership.Id == id));
            }

            public Task<UserTenantMembership?> GetByUserAndTenantAsync(
                Guid userId,
                Guid tenantId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Memberships.FirstOrDefault(membership =>
                    membership.UserId == userId && membership.TenantId == tenantId));
            }

            public Task<IReadOnlyList<UserTenantMembership>> GetByUserIdAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult<IReadOnlyList<UserTenantMembership>>(
                    Memberships.Where(membership => membership.UserId == userId).ToArray());
            }

            public Task<UserTenantMembership> AddAsync(
                UserTenantMembership membership,
                CancellationToken cancellationToken = default)
            {
                if (Memberships.All(existing => existing.Id != membership.Id))
                {
                    Memberships.Add(membership);
                }

                return Task.FromResult(membership);
            }

            public Task UpdateAsync(UserTenantMembership membership, CancellationToken cancellationToken = default)
            {
                UpdateCalls++;
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryBranchAssignmentStore : IUserBranchAssignmentStore
        {
            public List<UserBranchAssignment> Assignments { get; } = new();
            public int AddCalls { get; private set; }
            public int SaveChangesCalls { get; private set; }

            public Task<UserBranchAssignment?> GetByMembershipAndBranchAsync(
                Guid membershipId,
                Guid branchId,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Assignments.FirstOrDefault(assignment =>
                    assignment.MembershipId == membershipId && assignment.BranchId == branchId));
            }

            public Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default)
            {
                AddCalls++;
                if (Assignments.All(existing => existing.Id != assignment.Id))
                {
                    Assignments.Add(assignment);
                }

                return Task.CompletedTask;
            }

            public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                SaveChangesCalls++;
                return Task.CompletedTask;
            }
        }

        private sealed class InMemoryRoleRepository : IRoleRepository
        {
            public InMemoryRoleRepository(params Role[] roles)
            {
                Roles.AddRange(roles);
            }

            public List<Role> Roles { get; } = new();

            public Role GetRequired(string name)
            {
                return Roles.Single(role => role.Name == name);
            }

            public Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Roles.FirstOrDefault(role => role.Id == id));
            }

            public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(Roles.FirstOrDefault(role =>
                    role.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
            }

            public Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default)
            {
                Roles.Add(role);
                return Task.FromResult(role);
            }

            public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }
        }

        private sealed class RecordingPasswordHasher : IPasswordHasher
        {
            public List<string> HashedPasswords { get; } = new();

            public string HashPassword(string password)
            {
                HashedPasswords.Add(password);
                return $"hashed:{password}";
            }

            public bool VerifyPassword(string hashedPassword, string providedPassword)
            {
                return hashedPassword == $"hashed:{providedPassword}";
            }
        }
    }
}
