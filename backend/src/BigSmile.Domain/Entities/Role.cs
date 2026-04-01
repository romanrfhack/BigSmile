using BigSmile.SharedKernel;
using System;

namespace BigSmile.Domain.Entities
{
    public class Role : Entity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public bool IsSystem { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        // Navigation properties
        private readonly List<UserTenantMembership> _userTenantMemberships = new();
        public IReadOnlyCollection<UserTenantMembership> UserTenantMemberships => _userTenantMemberships.AsReadOnly();

        protected Role() { }

        public Role(string name, string? description = null, bool isSystem = false)
        {
            Id = Guid.NewGuid();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            IsSystem = isSystem;
        }

        // Constructor for seed data only
        public Role(Guid id, string name, string? description = null, bool isSystem = false)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            IsSystem = isSystem;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }
    }
}