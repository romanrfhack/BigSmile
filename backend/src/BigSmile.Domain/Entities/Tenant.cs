using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public class Tenant : Entity<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string? Subdomain { get; private set; }
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<Branch> _branches = new();
        public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

        protected Tenant() { }

        public Tenant(string name, string? subdomain = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Subdomain = subdomain;
        }

        public void UpdateName(string name)
        {
            Name = name;
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

        public Branch AddBranch(string name, string? address = null)
        {
            var branch = new Branch(this, name, address);
            _branches.Add(branch);
            UpdatedAt = DateTime.UtcNow;
            return branch;
        }
    }
}