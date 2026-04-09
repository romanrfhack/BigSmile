using BigSmile.Application.Features.Branches.Dtos;
using BigSmile.Application.Interfaces.Repositories;

namespace BigSmile.Application.Features.Branches.Queries
{
    public interface IBranchQueryService
    {
        Task<BranchDto?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class BranchQueryService : IBranchQueryService
    {
        private readonly IBranchRepository _branchRepository;

        public BranchQueryService(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository ?? throw new ArgumentNullException(nameof(branchRepository));
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
