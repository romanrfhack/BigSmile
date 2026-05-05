using BigSmile.Application.Authorization;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BigSmile.Infrastructure.Data
{
    public sealed class RealPilotUserBootstrapper
    {
        private const string DefaultTenantSubdomain = "default";
        private const string DefaultBranchName = "Sucursal Principal";

        private static readonly PilotUserDefinition[] PilotUsers =
        {
            new(
                "roman@bigsmile.com.mx",
                "Roman",
                SystemRoles.PlatformAdmin,
                RealPilotUserBootstrapOptions.RomanPasswordKey,
                RequiresBranchAssignment: false),
            new(
                "viridiana@bigsmile.com.mx",
                "Viridiana",
                SystemRoles.TenantAdmin,
                RealPilotUserBootstrapOptions.ViridianaPasswordKey,
                RequiresBranchAssignment: false),
            new(
                "jorge@bigsmile.com.mx",
                "Jorge",
                SystemRoles.TenantAdmin,
                RealPilotUserBootstrapOptions.JorgePasswordKey,
                RequiresBranchAssignment: false),
            new(
                "usuarioSucursal@bigsmile.com.mx",
                "Usuario Sucursal",
                SystemRoles.TenantUser,
                RealPilotUserBootstrapOptions.UsuarioSucursalPasswordKey,
                RequiresBranchAssignment: true)
        };

        private readonly ITenantRepository _tenantRepository;
        private readonly IBranchRepository _branchRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserTenantMembershipRepository _membershipRepository;
        private readonly IUserBranchAssignmentStore _branchAssignmentStore;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<RealPilotUserBootstrapper> _logger;

        public RealPilotUserBootstrapper(
            ITenantRepository tenantRepository,
            IBranchRepository branchRepository,
            IUserRepository userRepository,
            IUserTenantMembershipRepository membershipRepository,
            IUserBranchAssignmentStore branchAssignmentStore,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            ILogger<RealPilotUserBootstrapper> logger)
        {
            _tenantRepository = tenantRepository;
            _branchRepository = branchRepository;
            _userRepository = userRepository;
            _membershipRepository = membershipRepository;
            _branchAssignmentStore = branchAssignmentStore;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public static async Task BootstrapIfRequestedAsync(
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            var options = RealPilotUserBootstrapOptions.FromEnvironment();
            if (!options.Enabled)
            {
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var bootstrapper = scope.ServiceProvider.GetRequiredService<RealPilotUserBootstrapper>();
            await bootstrapper.BootstrapAsync(options, cancellationToken);
        }

        public async Task<RealPilotUserBootstrapResult> BootstrapAsync(
            RealPilotUserBootstrapOptions options,
            CancellationToken cancellationToken = default)
        {
            if (!options.Enabled)
            {
                _logger.LogDebug("Real pilot user bootstrap is disabled.");
                return RealPilotUserBootstrapResult.Empty;
            }

            var passwords = options.GetRequiredPasswords();
            var tenant = await ResolveTenantAsync(cancellationToken);
            var roles = await ResolveRolesAsync(cancellationToken);
            Branch? branchForTenantUser = null;
            var result = new RealPilotUserBootstrapResult();

            _logger.LogInformation(
                "Real pilot user bootstrap started for tenant {TenantId} with subdomain {TenantSubdomain}.",
                tenant.Id,
                tenant.Subdomain ?? "n/a");

            foreach (var pilotUser in PilotUsers)
            {
                var role = roles[pilotUser.RoleName];
                var password = passwords[pilotUser.PasswordConfigurationKey];
                var userResult = await EnsureUserAsync(pilotUser, password, cancellationToken);
                result.AddUserResult(userResult.WasCreated);

                var membership = await EnsureMembershipAsync(
                    userResult.User,
                    tenant,
                    role,
                    pilotUser,
                    result,
                    cancellationToken);

                if (pilotUser.RequiresBranchAssignment)
                {
                    branchForTenantUser ??= await EnsureDefaultBranchAsync(tenant, result, cancellationToken);
                    await EnsureBranchAssignmentAsync(membership, branchForTenantUser, result, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Real pilot user bootstrap finished. Users created: {UsersCreated}; users already existing: {UsersExisting}; memberships created: {MembershipsCreated}; memberships already existing: {MembershipsExisting}; branch created: {BranchCreated}; branch assignments created: {BranchAssignmentsCreated}; branch assignments already existing: {BranchAssignmentsExisting}.",
                result.UsersCreated,
                result.UsersExisting,
                result.MembershipsCreated,
                result.MembershipsExisting,
                result.BranchCreated,
                result.BranchAssignmentsCreated,
                result.BranchAssignmentsExisting);

            return result;
        }

        private async Task<Tenant> ResolveTenantAsync(CancellationToken cancellationToken)
        {
            var defaultTenant = await _tenantRepository.GetBySubdomainAsync(DefaultTenantSubdomain, cancellationToken);
            if (defaultTenant is { IsActive: true })
            {
                return defaultTenant;
            }

            var activeTenants = (await _tenantRepository.GetAllAsync(cancellationToken))
                .Where(tenant => tenant.IsActive)
                .ToArray();

            if (activeTenants.Length == 1)
            {
                var tenant = activeTenants[0];
                _logger.LogWarning(
                    "Default tenant with subdomain {DefaultTenantSubdomain} was not found. Using the only active tenant {TenantId} with subdomain {TenantSubdomain}.",
                    DefaultTenantSubdomain,
                    tenant.Id,
                    tenant.Subdomain ?? "n/a");
                return tenant;
            }

            throw new InvalidOperationException(
                "Real pilot user bootstrap requires an existing active default tenant with subdomain 'default', or exactly one active tenant in the database.");
        }

        private async Task<IReadOnlyDictionary<string, Role>> ResolveRolesAsync(CancellationToken cancellationToken)
        {
            var roles = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
            foreach (var roleName in PilotUsers.Select(user => user.RoleName).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
                if (role == null)
                {
                    throw new InvalidOperationException($"Real pilot user bootstrap requires existing role '{roleName}'.");
                }

                roles[roleName] = role;
            }

            return roles;
        }

        private async Task<PilotUserBootstrapResult> EnsureUserAsync(
            PilotUserDefinition pilotUser,
            string password,
            CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(pilotUser.Email, cancellationToken);
            if (existingUser != null)
            {
                if (!existingUser.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Pilot user '{pilotUser.Email}' already exists but is inactive. Reactivate or resolve that user explicitly before running the bootstrap.");
                }

                _logger.LogInformation(
                    "Pilot user {Email} already exists; password and profile fields were not changed.",
                    pilotUser.Email);
                return new PilotUserBootstrapResult(existingUser, WasCreated: false);
            }

            var hashedPassword = _passwordHasher.HashPassword(password);
            var user = new User(pilotUser.Email, hashedPassword, pilotUser.DisplayName);
            await _userRepository.AddAsync(user, cancellationToken);

            _logger.LogInformation("Pilot user {Email} was created.", pilotUser.Email);
            return new PilotUserBootstrapResult(user, WasCreated: true);
        }

        private async Task<UserTenantMembership> EnsureMembershipAsync(
            User user,
            Tenant tenant,
            Role role,
            PilotUserDefinition pilotUser,
            RealPilotUserBootstrapResult result,
            CancellationToken cancellationToken)
        {
            var membership = await _membershipRepository.GetByUserAndTenantAsync(
                user.Id,
                tenant.Id,
                cancellationToken);

            if (membership == null)
            {
                membership = user.AddTenantMembership(tenant, role);
                await _membershipRepository.AddAsync(membership, cancellationToken);
                result.MembershipsCreated++;

                _logger.LogInformation(
                    "Pilot user {Email} membership was created for tenant {TenantId} with role {RoleName}.",
                    pilotUser.Email,
                    tenant.Id,
                    role.Name);
                return membership;
            }

            if (!membership.Role.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Pilot user '{pilotUser.Email}' already has role '{membership.Role.Name}' for tenant '{tenant.Id}', but bootstrap requires role '{role.Name}'.");
            }

            if (!membership.IsActive)
            {
                membership.Activate();
                await _membershipRepository.UpdateAsync(membership, cancellationToken);
                _logger.LogInformation(
                    "Pilot user {Email} membership for tenant {TenantId} was reactivated.",
                    pilotUser.Email,
                    tenant.Id);
            }

            result.MembershipsExisting++;
            _logger.LogInformation(
                "Pilot user {Email} membership already exists for tenant {TenantId} with role {RoleName}.",
                pilotUser.Email,
                tenant.Id,
                role.Name);
            return membership;
        }

        private async Task<Branch> EnsureDefaultBranchAsync(
            Tenant tenant,
            RealPilotUserBootstrapResult result,
            CancellationToken cancellationToken)
        {
            var branches = await _branchRepository.GetByTenantIdAsync(tenant.Id, cancellationToken);
            var activeBranch = branches
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.CreatedAt)
                .ThenBy(branch => branch.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (activeBranch != null)
            {
                _logger.LogInformation(
                    "Using existing branch {BranchId} ({BranchName}) for branch-scoped pilot user.",
                    activeBranch.Id,
                    activeBranch.Name);
                return activeBranch;
            }

            var inactiveDefaultBranch = branches.FirstOrDefault(branch =>
                !branch.IsActive &&
                branch.Name.Equals(DefaultBranchName, StringComparison.OrdinalIgnoreCase));

            if (inactiveDefaultBranch != null)
            {
                inactiveDefaultBranch.Activate();
                await _branchRepository.UpdateAsync(inactiveDefaultBranch, cancellationToken);

                _logger.LogInformation(
                    "Reactivated default branch {BranchId} ({BranchName}) for branch-scoped pilot user.",
                    inactiveDefaultBranch.Id,
                    inactiveDefaultBranch.Name);
                return inactiveDefaultBranch;
            }

            var defaultBranch = tenant.AddBranch(DefaultBranchName);
            await _branchRepository.AddAsync(defaultBranch, cancellationToken);
            result.BranchCreated = true;

            _logger.LogInformation(
                "Created default branch {BranchId} ({BranchName}) for branch-scoped pilot user.",
                defaultBranch.Id,
                defaultBranch.Name);
            return defaultBranch;
        }

        private async Task EnsureBranchAssignmentAsync(
            UserTenantMembership membership,
            Branch branch,
            RealPilotUserBootstrapResult result,
            CancellationToken cancellationToken)
        {
            var existingAssignment = await _branchAssignmentStore.GetByMembershipAndBranchAsync(
                membership.Id,
                branch.Id,
                cancellationToken);

            if (existingAssignment == null)
            {
                var assignment = membership.AssignToBranch(branch);
                await _branchAssignmentStore.AddAsync(assignment, cancellationToken);
                result.BranchAssignmentsCreated++;

                _logger.LogInformation(
                    "Pilot user {Email} was assigned to branch {BranchId} ({BranchName}).",
                    membership.User.Email,
                    branch.Id,
                    branch.Name);
                return;
            }

            if (!existingAssignment.IsActive)
            {
                existingAssignment.Activate();
                await _branchAssignmentStore.SaveChangesAsync(cancellationToken);
                result.BranchAssignmentsReactivated++;

                _logger.LogInformation(
                    "Pilot user {Email} branch assignment was reactivated for branch {BranchId} ({BranchName}).",
                    membership.User.Email,
                    branch.Id,
                    branch.Name);
                return;
            }

            result.BranchAssignmentsExisting++;
            _logger.LogInformation(
                "Pilot user {Email} already has an active branch assignment for branch {BranchId} ({BranchName}).",
                membership.User.Email,
                branch.Id,
                branch.Name);
        }

        private sealed record PilotUserDefinition(
            string Email,
            string DisplayName,
            string RoleName,
            string PasswordConfigurationKey,
            bool RequiresBranchAssignment);

        private sealed record PilotUserBootstrapResult(User User, bool WasCreated);
    }

    public sealed class RealPilotUserBootstrapOptions
    {
        public const string EnabledKey = "BIGSMILE_BOOTSTRAP_REAL_USERS";
        public const string RomanPasswordKey = "BIGSMILE_BOOTSTRAP_ROMAN_PASSWORD";
        public const string ViridianaPasswordKey = "BIGSMILE_BOOTSTRAP_VIRIDIANA_PASSWORD";
        public const string JorgePasswordKey = "BIGSMILE_BOOTSTRAP_JORGE_PASSWORD";
        public const string UsuarioSucursalPasswordKey = "BIGSMILE_BOOTSTRAP_USUARIO_SUCURSAL_PASSWORD";

        public bool Enabled { get; init; }
        public string? RomanPassword { get; init; }
        public string? ViridianaPassword { get; init; }
        public string? JorgePassword { get; init; }
        public string? UsuarioSucursalPassword { get; init; }

        public static RealPilotUserBootstrapOptions FromEnvironment()
        {
            return new RealPilotUserBootstrapOptions
            {
                Enabled = bool.TryParse(Environment.GetEnvironmentVariable(EnabledKey), out var enabled) && enabled,
                RomanPassword = Environment.GetEnvironmentVariable(RomanPasswordKey),
                ViridianaPassword = Environment.GetEnvironmentVariable(ViridianaPasswordKey),
                JorgePassword = Environment.GetEnvironmentVariable(JorgePasswordKey),
                UsuarioSucursalPassword = Environment.GetEnvironmentVariable(UsuarioSucursalPasswordKey)
            };
        }

        public IReadOnlyDictionary<string, string> GetRequiredPasswords()
        {
            var passwords = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [RomanPasswordKey] = RomanPassword ?? string.Empty,
                [ViridianaPasswordKey] = ViridianaPassword ?? string.Empty,
                [JorgePasswordKey] = JorgePassword ?? string.Empty,
                [UsuarioSucursalPasswordKey] = UsuarioSucursalPassword ?? string.Empty
            };

            var missingKeys = passwords
                .Where(pair => string.IsNullOrWhiteSpace(pair.Value))
                .Select(pair => pair.Key)
                .ToArray();

            if (missingKeys.Length > 0)
            {
                throw new InvalidOperationException(
                    "Real pilot user bootstrap is enabled but required password environment variables are missing: " +
                    string.Join(", ", missingKeys) +
                    ".");
            }

            return passwords;
        }
    }

    public sealed class RealPilotUserBootstrapResult
    {
        public static RealPilotUserBootstrapResult Empty => new();

        public int UsersCreated { get; private set; }
        public int UsersExisting { get; private set; }
        public int MembershipsCreated { get; internal set; }
        public int MembershipsExisting { get; internal set; }
        public bool BranchCreated { get; internal set; }
        public int BranchAssignmentsCreated { get; internal set; }
        public int BranchAssignmentsReactivated { get; internal set; }
        public int BranchAssignmentsExisting { get; internal set; }

        internal void AddUserResult(bool wasCreated)
        {
            if (wasCreated)
            {
                UsersCreated++;
                return;
            }

            UsersExisting++;
        }
    }
}
