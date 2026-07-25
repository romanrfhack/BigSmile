using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientPortalSecurityAuditEntry : Entity<Guid>, ITenantOwnedEntity
    {
        public const int ActionMaxLength = 40;
        public const int CorrelationIdMaxLength = 100;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public Guid PatientPortalInvitationId { get; private set; }
        public PatientPortalInvitation PatientPortalInvitation { get; private set; } = null!;

        public PatientPortalSecurityAuditAction Action { get; private set; }
        public Guid ActorUserId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string CorrelationId { get; private set; } = string.Empty;

        private PatientPortalSecurityAuditEntry()
        {
        }

        public PatientPortalSecurityAuditEntry(
            PatientPortalInvitation invitation,
            PatientPortalSecurityAuditAction action,
            Guid actorUserId,
            DateTime occurredAtUtc,
            string correlationId)
        {
            ArgumentNullException.ThrowIfNull(invitation);

            if (!Enum.IsDefined(action))
            {
                throw new ArgumentException("Patient portal security audit action is not supported.", nameof(action));
            }

            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal security audit actor is required.", nameof(actorUserId));
            }

            if (occurredAtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Patient portal security audit timestamp must be UTC.", nameof(occurredAtUtc));
            }

            Id = Guid.NewGuid();
            TenantId = invitation.TenantId;
            PatientId = invitation.PatientId;
            PatientPortalInvitationId = invitation.Id;
            PatientPortalInvitation = invitation;
            Action = action;
            ActorUserId = actorUserId;
            OccurredAtUtc = occurredAtUtc;
            CorrelationId = NormalizeCorrelationId(correlationId);
        }

        private static string NormalizeCorrelationId(string? correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Patient portal security audit correlation id is required.", nameof(correlationId));
            }

            var normalized = correlationId.Trim();
            if (normalized.Length > CorrelationIdMaxLength)
            {
                throw new ArgumentException(
                    $"Patient portal security audit correlation id cannot exceed {CorrelationIdMaxLength} characters.",
                    nameof(correlationId));
            }

            return normalized;
        }
    }
}
