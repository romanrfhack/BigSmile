using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public class Branch : Entity<Guid>
    {
        public Tenant Tenant { get; private set; } = null!;
        public Guid TenantId { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Address { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        protected Branch() { }

        internal Branch(Tenant tenant, string name, string? address = null)
        {
            Id = Guid.NewGuid();
            Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
            TenantId = tenant.Id;
            Name = name;
            Address = address;
        }

        public void UpdateName(string name)
        {
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAddress(string? address)
        {
            Address = address;
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
    }
}