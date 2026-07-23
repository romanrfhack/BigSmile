using BigSmile.SharedKernel;

namespace BigSmile.Domain.Entities
{
    public class Tenant : Entity<Guid>
    {
        public const string DefaultTimeZoneId = "America/Mexico_City";
        public const int TimeZoneIdMaxLength = 100;

        public string Name { get; private set; } = string.Empty;
        public string? Subdomain { get; private set; }
        public string TimeZoneId { get; private set; } = DefaultTimeZoneId;
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        private readonly List<Branch> _branches = new();
        public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

        protected Tenant() { }

        public Tenant(string name, string? subdomain = null, string? timeZoneId = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Subdomain = subdomain;
            TimeZoneId = NormalizeTimeZoneId(timeZoneId);
        }

        public void UpdateName(string name)
        {
            Name = name;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateTimeZone(string timeZoneId)
        {
            var normalizedTimeZoneId = NormalizeTimeZoneId(timeZoneId);
            if (TimeZoneId == normalizedTimeZoneId)
            {
                return;
            }

            TimeZoneId = normalizedTimeZoneId;
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

        private static string NormalizeTimeZoneId(string? timeZoneId)
        {
            var normalized = string.IsNullOrWhiteSpace(timeZoneId)
                ? DefaultTimeZoneId
                : timeZoneId.Trim();

            if (normalized.Length > TimeZoneIdMaxLength)
            {
                throw new ArgumentException(
                    $"Tenant time zone id cannot exceed {TimeZoneIdMaxLength} characters.",
                    nameof(timeZoneId));
            }

            try
            {
                _ = TimeZoneInfo.FindSystemTimeZoneById(normalized);
            }
            catch (TimeZoneNotFoundException exception)
            {
                throw new ArgumentException(
                    $"Tenant time zone id '{normalized}' is not available on the current platform.",
                    nameof(timeZoneId),
                    exception);
            }
            catch (InvalidTimeZoneException exception)
            {
                throw new ArgumentException(
                    $"Tenant time zone id '{normalized}' is invalid on the current platform.",
                    nameof(timeZoneId),
                    exception);
            }

            return normalized;
        }
    }
}
