using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class ReminderTemplate : Entity<Guid>, ITenantOwnedEntity
    {
        public const int NameMaxLength = 120;
        public const int BodyMaxLength = 1000;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public string Name { get; private set; } = string.Empty;
        public string Body { get; private set; } = string.Empty;
        public bool IsActive { get; private set; } = true;
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public Guid? UpdatedByUserId { get; private set; }
        public DateTime? DeactivatedAtUtc { get; private set; }
        public Guid? DeactivatedByUserId { get; private set; }

        private ReminderTemplate()
        {
        }

        public ReminderTemplate(
            Guid tenantId,
            string name,
            string body,
            Guid createdByUserId,
            DateTime? createdAtUtc = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Reminder template tenant ownership is required.", nameof(tenantId));
            }

            EnsureActor(createdByUserId, nameof(createdByUserId));

            Id = Guid.NewGuid();
            TenantId = tenantId;
            Name = NormalizeRequired(name, nameof(name), NameMaxLength);
            Body = NormalizeRequired(body, nameof(body), BodyMaxLength);
            IsActive = true;
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
        }

        public void Update(string name, string body, Guid updatedByUserId)
        {
            EnsureActor(updatedByUserId, nameof(updatedByUserId));

            if (!IsActive)
            {
                throw new InvalidOperationException("Inactive reminder templates cannot be updated.");
            }

            Name = NormalizeRequired(name, nameof(name), NameMaxLength);
            Body = NormalizeRequired(body, nameof(body), BodyMaxLength);
            UpdatedAtUtc = DateTime.UtcNow;
            UpdatedByUserId = updatedByUserId;
        }

        public void Deactivate(Guid deactivatedByUserId)
        {
            EnsureActor(deactivatedByUserId, nameof(deactivatedByUserId));

            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            DeactivatedAtUtc = DateTime.UtcNow;
            DeactivatedByUserId = deactivatedByUserId;
        }

        private static void EnsureActor(Guid actorUserId, string paramName)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Reminder template actor is required.", paramName);
            }
        }

        private static string NormalizeRequired(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{paramName} is required.", paramName);
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} exceeds the allowed length of {maxLength}.", paramName);
            }

            return normalized;
        }
    }
}
