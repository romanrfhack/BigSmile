using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientPortalAccount : Entity<Guid>, ITenantOwnedEntity
    {
        public const int LoginNameMinLength = 3;
        public const int LoginNameMaxLength = 200;
        public const int PasswordHashMaxLength = 512;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid? PatientId { get; private set; }
        public Patient? Patient { get; private set; }

        public string LoginName { get; private set; } = string.Empty;
        public string NormalizedLoginName { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public int FailedLoginAttempts { get; private set; }
        public DateTime? LockoutEndUtc { get; private set; }
        public DateTime? LastFailedLoginAtUtc { get; private set; }
        public DateTime? LastSuccessfulLoginAtUtc { get; private set; }
        public int SessionVersion { get; private set; } = 1;
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime LastUpdatedAtUtc { get; private set; }
        public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

        private PatientPortalAccount()
        {
        }

        public PatientPortalAccount(
            Guid tenantId,
            Guid? patientId,
            string loginName,
            string passwordHash)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal account tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal account patient reference cannot be empty.", nameof(patientId));
            }

            var normalizedLoginName = NormalizeLoginName(loginName);
            var now = DateTime.UtcNow;

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            LoginName = normalizedLoginName;
            NormalizedLoginName = normalizedLoginName.ToUpperInvariant();
            PasswordHash = NormalizePasswordHash(passwordHash);
            CreatedAtUtc = now;
            LastUpdatedAtUtc = now;
        }

        public void LinkPatient(Guid patientId)
        {
            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal account patient reference is required.", nameof(patientId));
            }

            if (PatientId.HasValue && PatientId.Value != patientId)
            {
                throw new InvalidOperationException("Patient portal account is already linked to another patient.");
            }

            if (PatientId == patientId)
            {
                return;
            }

            PatientId = patientId;
            LastUpdatedAtUtc = DateTime.UtcNow;
        }

        public bool RegisterFailedLogin(
            DateTime occurredAtUtc,
            int maxFailedAttempts,
            TimeSpan lockoutDuration)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            if (maxFailedAttempts <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFailedAttempts));
            }

            if (lockoutDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(lockoutDuration));
            }

            if (IsLockedOutAt(occurredAtUtc))
            {
                return true;
            }

            FailedLoginAttempts = Math.Min(maxFailedAttempts, FailedLoginAttempts + 1);
            LastFailedLoginAtUtc = occurredAtUtc;
            LastUpdatedAtUtc = occurredAtUtc;

            if (FailedLoginAttempts < maxFailedAttempts)
            {
                return false;
            }

            LockoutEndUtc = occurredAtUtc.Add(lockoutDuration);
            return true;
        }

        public void RegisterSuccessfulLogin(DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            FailedLoginAttempts = 0;
            LockoutEndUtc = null;
            LastSuccessfulLoginAtUtc = occurredAtUtc;
            LastUpdatedAtUtc = occurredAtUtc;
        }

        public bool IsLockedOutAt(DateTime utcNow)
        {
            EnsureUtc(utcNow, nameof(utcNow));
            return LockoutEndUtc.HasValue && LockoutEndUtc.Value > utcNow;
        }

        public void RecoverAccess(string passwordHash, DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            PasswordHash = NormalizePasswordHash(passwordHash);
            FailedLoginAttempts = 0;
            LockoutEndUtc = null;
            LastFailedLoginAtUtc = null;
            IsActive = true;
            SessionVersion = checked(SessionVersion + 1);
            LastUpdatedAtUtc = occurredAtUtc;
        }

        public void RevokeSessions(DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));
            SessionVersion = checked(SessionVersion + 1);
            LastUpdatedAtUtc = occurredAtUtc;
        }

        public void Deactivate(DateTime occurredAtUtc)
        {
            EnsureUtc(occurredAtUtc, nameof(occurredAtUtc));

            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            SessionVersion = checked(SessionVersion + 1);
            LastUpdatedAtUtc = occurredAtUtc;
        }

        private static string NormalizeLoginName(string? loginName)
        {
            if (string.IsNullOrWhiteSpace(loginName))
            {
                throw new ArgumentException("Patient portal login name is required.", nameof(loginName));
            }

            var normalized = loginName.Trim();
            if (normalized.Length is < LoginNameMinLength or > LoginNameMaxLength)
            {
                throw new ArgumentException(
                    $"Patient portal login name must contain between {LoginNameMinLength} and {LoginNameMaxLength} characters.",
                    nameof(loginName));
            }

            if (normalized.Any(character => char.IsWhiteSpace(character) || char.IsControl(character)))
            {
                throw new ArgumentException(
                    "Patient portal login name cannot contain whitespace or control characters.",
                    nameof(loginName));
            }

            return normalized;
        }

        private static string NormalizePasswordHash(string? passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Patient portal password hash is required.", nameof(passwordHash));
            }

            var normalized = passwordHash.Trim();
            if (normalized.Length > PasswordHashMaxLength)
            {
                throw new ArgumentException(
                    $"Patient portal password hash cannot exceed {PasswordHashMaxLength} characters.",
                    nameof(passwordHash));
            }

            return normalized;
        }

        private static void EnsureUtc(DateTime value, string paramName)
        {
            if (value.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Patient portal security timestamps must be UTC.", paramName);
            }
        }
    }
}
