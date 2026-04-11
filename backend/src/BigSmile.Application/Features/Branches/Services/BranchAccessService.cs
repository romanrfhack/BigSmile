using BigSmile.Application.Authorization;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.Branches.Services
{
    public interface IBranchAccessService
    {
        Task<IReadOnlyList<Branch>> GetAccessibleBranchesAsync(CancellationToken cancellationToken = default);
        Task<Branch?> GetAccessibleBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    }

    public sealed class BranchAccessService : IBranchAccessService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IUserTenantMembershipRepository _membershipRepository;
        private readonly IRolePermissionCatalog _permissionCatalog;
        private readonly ITenantContext _tenantContext;

        public BranchAccessService(
            IBranchRepository branchRepository,
            IUserTenantMembershipRepository membershipRepository,
            IRolePermissionCatalog permissionCatalog,
            ITenantContext tenantContext)
        {
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
            _membershipRepository = membershipRepository ?? throw new ArgumentNullException(nameof(membershipRepository));
            _permissionCatalog = permissionCatalog ?? throw new ArgumentNullException(nameof(permissionCatalog));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<IReadOnlyList<Branch>> GetAccessibleBranchesAsync(CancellationToken cancellationToken = default)
        {
            if (!TryGetTenantAndUserIds(out var tenantId, out var userId))
            {
                return Array.Empty<Branch>();
            }

            var membership = await _membershipRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (membership == null || !membership.IsActive)
            {
                return Array.Empty<Branch>();
            }

            var branches = await _branchRepository.GetByTenantIdAsync(tenantId, cancellationToken);
            return FilterBranches(branches, membership)
                .Where(branch => branch.IsActive)
                .OrderBy(branch => branch.Name)
                .ToArray();
        }

        public async Task<Branch?> GetAccessibleBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
        {
            var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken);
            if (branch == null)
            {
                return null;
            }

            if (_tenantContext.HasPlatformOverride())
            {
                return branch;
            }

            if (!TryGetTenantAndUserIds(out var tenantId, out var userId) || branch.TenantId != tenantId)
            {
                return null;
            }

            var membership = await _membershipRepository.GetByUserAndTenantAsync(userId, tenantId, cancellationToken);
            if (membership == null || !membership.IsActive)
            {
                return null;
            }

            return FilterBranches(new[] { branch }, membership).FirstOrDefault();
        }

        private IEnumerable<Branch> FilterBranches(IEnumerable<Branch> branches, UserTenantMembership membership)
        {
            var permissions = _permissionCatalog.GetPermissions(membership.Role.Name);
            if (permissions.Contains(Permissions.BranchReadAny, StringComparer.OrdinalIgnoreCase))
            {
                return branches;
            }

            if (!permissions.Contains(Permissions.BranchReadAssigned, StringComparer.OrdinalIgnoreCase))
            {
                return Array.Empty<Branch>();
            }

            var assignedBranchIds = membership.BranchAssignments
                .Where(assignment => assignment.IsActive)
                .Select(assignment => assignment.BranchId)
                .ToHashSet();

            return branches.Where(branch => assignedBranchIds.Contains(branch.Id));
        }

        private bool TryGetTenantAndUserIds(out Guid tenantId, out Guid userId)
        {
            tenantId = Guid.Empty;
            userId = Guid.Empty;

            return Guid.TryParse(_tenantContext.GetTenantId(), out tenantId)
                && tenantId != Guid.Empty
                && Guid.TryParse(_tenantContext.GetUserId(), out userId)
                && userId != Guid.Empty;
        }
    }
}
