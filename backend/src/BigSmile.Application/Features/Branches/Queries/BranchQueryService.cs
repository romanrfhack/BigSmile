using BigSmile.Application.Features.Branches.Dtos;
using BigSmile.Application.Features.Branches.Services;
using BigSmile.Application.Interfaces.Repositories;

namespace BigSmile.Application.Features.Branches.Queries
{
    public interface IBranchQueryService
    {
        Task<IReadOnlyList<BranchDto>> GetAccessibleBranchesAsync(CancellationToken cancellationToken = default);
        Task<BranchDto?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class BranchQueryService : IBranchQueryService
    {
        private readonly IBranchRepository _branchRepository;
        private readonly IBranchAccessService _branchAccessService;

        public BranchQueryService(
            IBranchRepository branchRepository,
            IBranchAccessService branchAccessService)
        {
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
            _branchAccessService = branchAccessService ?? throw new ArgumentNullException(nameof(branchAccessService));
        }

        public async Task<IReadOnlyList<BranchDto>> GetAccessibleBranchesAsync(CancellationToken cancellationToken = default)
        {
            var branches = await _branchAccessService.GetAccessibleBranchesAsync(cancellationToken);

            return branches
                .Select(branch => new BranchDto(
                    branch.Id,
                    branch.TenantId,
                    branch.Name,
                    branch.Address,
                    branch.IsActive,
                    branch.CreatedAt,
                    branch.UpdatedAt))
                .ToArray();
        }

        public async Task<BranchDto?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var branch = await _branchRepository.GetByIdAsync(id, cancellationToken);
            if (branch == null)
            {
                return null;
            }

            return new BranchDto(
                branch.Id,
                branch.TenantId,
                branch.Name,
                branch.Address,
                branch.IsActive,
                branch.CreatedAt,
                branch.UpdatedAt);
        }
    }
}
