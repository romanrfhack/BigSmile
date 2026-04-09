using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;
using System;

namespace BigSmile.Domain.Entities
{
    public class UserTenantMembership : Entity<Guid>, ITenantOwnedEntity
    {
        public User User { get; private set; } = null!;
        public Guid UserId { get; private set; }

        public Tenant Tenant { get; private set; } = null!;
        public Guid TenantId { get; private set; }

        public Role Role { get; private set; } = null!;
        public Guid RoleId { get; private set; }

        public bool IsActive { get; private set; } = true;
        public DateTime JoinedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; private set; }

        // Navigation property for branch assignments (optional)
        private readonly List<UserBranchAssignment> _branchAssignments = new();
        public IReadOnlyCollection<UserBranchAssignment> BranchAssignments => _branchAssignments.AsReadOnly();

        protected UserTenantMembership() { }

        internal UserTenantMembership(User user, Tenant tenant, Role role)
        {
            Id = Guid.NewGuid();
            User = user ?? throw new ArgumentNullException(nameof(user));
            UserId = user.Id;
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            TenantId = tenant.Id;
            Role = role ?? throw new ArgumentNullException(nameof(role));
            RoleId = role.Id;
        }

        public void Deactivate()
        {
            IsActive = false;
            LeftAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            LeftAt = null;
        }

        public UserBranchAssignment AssignToBranch(Branch branch)
        {
            var assignment = new UserBranchAssignment(this, branch);
            _branchAssignments.Add(assignment);
            return assignment;
        }
    }
}
