using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientPortalAuthenticationAuditEntry : Entity<Guid>, ITenantOwnedEntity
    {
        public const int ActionMaxLength = 40;
        public const int ActorTypeMaxLength = 40;
        public const int CorrelationIdMaxLength = 100;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public Guid PatientPortalAccountId { get; private set; }
        public PatientPortalAccount PatientPortalAccount { get; private set; } = null!;

        public Guid? PatientPortalInvitationId { get; private set; }
        public PatientPortalInvitation? PatientPortalInvitation { get; private set; }

        public PatientPortalAuthenticationAuditAction Action { get; private set; }
        public PatientPortalAuthenticationAuditActorType ActorType { get; private set; }
        public Guid ActorId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string CorrelationId { get; private set; } = string.Empty;

        private PatientPortalAuthenticationAuditEntry()
        {
        }

        public PatientPortalAuthenticationAuditEntry(
            PatientPortalAccount account,
            PatientPortalAuthenticationAuditAction action,
            PatientPortalAuthenticationAuditActorType actorType,
            Guid actorId,
            DateTime occurredAtUtc,
            string correlationId,
            PatientPortalInvitation? invitation = null)
        {
            ArgumentNullException.ThrowIfNull(account);

            if (!account.PatientId.HasValue || account.PatientId.Value == Guid.Empty)
            {
                throw new ArgumentException(
                    "Patient portal authentication audit requires an account linked to a patient.",
                    nameof(account));
            }

            if (!Enum.IsDefined(action))
            {
                throw new ArgumentException("Patient portal authentication audit action is not supported.", nameof(action));
            }

            if (!Enum.IsDefined(actorType))
            {
                throw new ArgumentException("Patient portal authentication audit actor type is not supported.", nameof(actorType));
            }

            if (actorId == Guid.Empty)
            {
                throw new ArgumentException("Patient portal authentication audit actor is required.", nameof(actorId));
            }

            if (occurredAtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Patient portal authentication audit timestamp must be UTC.", nameof(occurredAtUtc));
            }

            if (invitation is not null &&
                (invitation.TenantId != account.TenantId || invitation.PatientId != account.PatientId.Value))
            {
                throw new ArgumentException(
                    "Patient portal authentication audit invitation must belong to the account tenant and patient.",
                    nameof(invitation));
            }

            Id = Guid.NewGuid();
            TenantId = account.TenantId;
            PatientId = account.PatientId.Value;
            PatientPortalAccountId = account.Id;
            PatientPortalAccount = account;
            PatientPortalInvitationId = invitation?.Id;
            PatientPortalInvitation = invitation;
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
                throw new ArgumentException("Patient portal authentication audit correlation id is required.", nameof(correlationId));
            }

            var normalized = correlationId.Trim();
            if (normalized.Length > CorrelationIdMaxLength)
            {
                throw new ArgumentException(
                    $"Patient portal authentication audit correlation id cannot exceed {CorrelationIdMaxLength} characters.",
                    nameof(correlationId));
            }

            return normalized;
        }
    }
}
