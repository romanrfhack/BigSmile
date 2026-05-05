using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BigSmile.Infrastructure.Data
{
    public interface IUserBranchAssignmentStore
    {
        Task<UserBranchAssignment?> GetByMembershipAndBranchAsync(
            Guid membershipId,
            Guid branchId,
            CancellationToken cancellationToken = default);

        Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }

    public sealed class EfUserBranchAssignmentStore : IUserBranchAssignmentStore
    {
        private readonly AppDbContext _dbContext;

        public EfUserBranchAssignmentStore(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserBranchAssignment?> GetByMembershipAndBranchAsync(
            Guid membershipId,
            Guid branchId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.UserBranchAssignments
                .FirstOrDefaultAsync(
                    assignment => assignment.MembershipId == membershipId && assignment.BranchId == branchId,
                    cancellationToken);
        }

        public async Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default)
        {
            _dbContext.UserBranchAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
