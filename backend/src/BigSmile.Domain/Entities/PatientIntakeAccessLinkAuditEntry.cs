using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientIntakeAccessLinkAuditEntry : Entity<Guid>, ITenantOwnedEntity
    {
        public const int ActionMaxLength = 40;
        public const int ActorTypeMaxLength = 40;
        public const int CorrelationIdMaxLength = 100;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientIntakeAccessLinkId { get; private set; }
        public PatientIntakeAccessLink PatientIntakeAccessLink { get; private set; } = null!;

        public Guid? BranchId { get; private set; }
        public PatientIntakeAccessLinkAuditAction Action { get; private set; }
        public PatientIntakeAccessLinkAuditActorType ActorType { get; private set; }
        public Guid ActorId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string CorrelationId { get; private set; } = string.Empty;

        private PatientIntakeAccessLinkAuditEntry()
        {
        }

        public PatientIntakeAccessLinkAuditEntry(
            PatientIntakeAccessLink accessLink,
            PatientIntakeAccessLinkAuditAction action,
            PatientIntakeAccessLinkAuditActorType actorType,
            Guid actorId,
            DateTime occurredAtUtc,
            string correlationId)
        {
            ArgumentNullException.ThrowIfNull(accessLink);

            if (!Enum.IsDefined(action))
            {
                throw new ArgumentException(
                    "Patient intake access link audit action is not supported.",
                    nameof(action));
            }

            if (!Enum.IsDefined(actorType))
            {
                throw new ArgumentException(
                    "Patient intake access link audit actor type is not supported.",
                    nameof(actorType));
            }

            if (actorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Patient intake access link audit actor is required.",
                    nameof(actorId));
            }

            if (occurredAtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient intake access link audit timestamp must be UTC.",
                    nameof(occurredAtUtc));
            }

            Id = Guid.NewGuid();
            TenantId = accessLink.TenantId;
            PatientIntakeAccessLinkId = accessLink.Id;
            PatientIntakeAccessLink = accessLink;
            BranchId = accessLink.BranchId;
            Action = action;
            ActorType = actorType;
            ActorId = actorId;
            OccurredAtUtc = occurredAtUtc;
            CorrelationId = NormalizeCorrelationId(correlationId);
        }

        private static string NormalizeCorrelationId(string? correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException(
                    "Patient intake access link audit correlation id is required.",
                    nameof(correlationId));
            }

            var normalized = correlationId.Trim();
            if (normalized.Length > CorrelationIdMaxLength)
            {
                throw new ArgumentException(
                    $"Patient intake access link audit correlation id cannot exceed {CorrelationIdMaxLength} characters.",
                    nameof(correlationId));
            }

            return normalized;
        }
    }
}
