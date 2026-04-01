using BigSmile.SharedKernel;
using System;

namespace BigSmile.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public string Email { get; private set; } = string.Empty;
        public string NormalizedEmail { get; private set; } = string.Empty;
        public string HashedPassword { get; private set; } = string.Empty;
        public string? DisplayName { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        // Navigation properties
        private readonly List<UserTenantMembership> _tenantMemberships = new();
        public IReadOnlyCollection<UserTenantMembership> TenantMemberships => _tenantMemberships.AsReadOnly();

        protected User() { }

        public User(string email, string hashedPassword, string? displayName = null)
        {
            Id = Guid.NewGuid();
            Email = email ?? throw new ArgumentNullException(nameof(email));
            NormalizedEmail = email.ToUpperInvariant();
            HashedPassword = hashedPassword ?? throw new ArgumentNullException(nameof(hashedPassword));
            DisplayName = displayName;
        }

        public void UpdateEmail(string email)
        {
            Email = email ?? throw new ArgumentNullException(nameof(email));
            NormalizedEmail = email.ToUpperInvariant();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePassword(string hashedPassword)
        {
            HashedPassword = hashedPassword ?? throw new ArgumentNullException(nameof(hashedPassword));
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDisplayName(string? displayName)
        {
            DisplayName = displayName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public UserTenantMembership AddTenantMembership(Tenant tenant, Role role)
        {
            var membership = new UserTenantMembership(this, tenant, role);
            _tenantMemberships.Add(membership);
            return membership;
        }
    }
}