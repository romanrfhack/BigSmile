using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Domain.Entities;

namespace BigSmile.Infrastructure.Data.Repositories
{
    public class InMemoryBranchRepository : IBranchRepository
    {
        private static readonly Dictionary<Guid, Branch> _branches = new();

        public Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _branches.TryGetValue(id, out var branch);
            return Task.FromResult(branch);
        }

        public Task<IReadOnlyList<Branch>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            var list = _branches.Values.Where(b => b.TenantId == tenantId).ToList();
            return Task.FromResult<IReadOnlyList<Branch>>(list);
        }

        public Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            if (_branches.ContainsKey(branch.Id))
                throw new InvalidOperationException($"Branch with id {branch.Id} already exists.");
            _branches.Add(branch.Id, branch);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            if (!_branches.ContainsKey(branch.Id))
                throw new InvalidOperationException($"Branch with id {branch.Id} not found.");
            _branches[branch.Id] = branch;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _branches.Remove(id);
            return Task.CompletedTask;
        }
    }
}