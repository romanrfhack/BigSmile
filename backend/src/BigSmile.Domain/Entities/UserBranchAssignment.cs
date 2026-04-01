using BigSmile.SharedKernel;
using System;

namespace BigSmile.Domain.Entities
{
    public class UserBranchAssignment : Entity<Guid>
    {
        public UserTenantMembership Membership { get; private set; } = null!;
        public Guid MembershipId { get; private set; }

        public Branch Branch { get; private set; } = null!;
        public Guid BranchId { get; private set; }

        public bool IsActive { get; private set; } = true;
        public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UnassignedAt { get; private set; }

        protected UserBranchAssignment() { }

        internal UserBranchAssignment(UserTenantMembership membership, Branch branch)
        {
            Id = Guid.NewGuid();
            Membership = membership ?? throw new ArgumentNullException(nameof(membership));
            MembershipId = membership.Id;
            Branch = branch ?? throw new ArgumentNullException(nameof(branch));
            BranchId = branch.Id;
        }

        public void Deactivate()
        {
            IsActive = false;
            UnassignedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UnassignedAt = null;
        }
    }
}