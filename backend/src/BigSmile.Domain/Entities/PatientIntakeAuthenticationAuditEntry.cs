using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientIntakeAuthenticationAuditEntry
        : Entity<Guid>, ITenantOwnedEntity
    {
        public const int ActionMaxLength = 40;
        public const int ActorTypeMaxLength = 40;
        public const int CorrelationIdMaxLength = 100;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientPortalAccountId { get; private set; }
        public PatientPortalAccount PatientPortalAccount { get; private set; } = null!;

        public Guid PatientIntakeId { get; private set; }
        public PatientIntake PatientIntake { get; private set; } = null!;

        public Guid? PatientIntakeAccessLinkId { get; private set; }
        public PatientIntakeAccessLink? PatientIntakeAccessLink { get; private set; }

        public PatientIntakeAuthenticationAuditAction Action { get; private set; }
        public PatientIntakeAuthenticationAuditActorType ActorType { get; private set; }
        public Guid ActorId { get; private set; }
        public DateTime OccurredAtUtc { get; private set; }
        public string CorrelationId { get; private set; } = string.Empty;

        private PatientIntakeAuthenticationAuditEntry()
        {
        }

        public PatientIntakeAuthenticationAuditEntry(
            PatientPortalAccount account,
            PatientIntake intake,
            PatientIntakeAuthenticationAuditAction action,
            PatientIntakeAuthenticationAuditActorType actorType,
            Guid actorId,
            DateTime occurredAtUtc,
            string correlationId,
            PatientIntakeAccessLink? accessLink = null)
        {
            ArgumentNullException.ThrowIfNull(account);
            ArgumentNullException.ThrowIfNull(intake);

            if (account.TenantId != intake.TenantId ||
                account.Id != intake.PatientPortalAccountId)
            {
                throw new InvalidOperationException(
                    "Patient intake authentication audit account and intake ownership must match.");
            }

            if (accessLink is not null &&
                (accessLink.TenantId != account.TenantId ||
                 accessLink.BranchId != intake.BranchId))
            {
                throw new InvalidOperationException(
                    "Patient intake authentication audit access link ownership must match the intake.");
            }

            if (!Enum.IsDefined(action))
            {
                throw new ArgumentException(
                    "Patient intake authentication audit action is not supported.",
                    nameof(action));
            }

            if (!Enum.IsDefined(actorType))
            {
                throw new ArgumentException(
                    "Patient intake authentication audit actor type is not supported.",
                    nameof(actorType));
            }

            if (actorId == Guid.Empty)
            {
                throw new ArgumentException(
                    "Patient intake authentication audit actor is required.",
                    nameof(actorId));
            }

            if (occurredAtUtc.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException(
                    "Patient intake authentication audit timestamp must be UTC.",
                    nameof(occurredAtUtc));
            }

            Id = Guid.NewGuid();
            TenantId = account.TenantId;
            PatientPortalAccountId = account.Id;
            PatientIntakeId = intake.Id;
            PatientIntakeAccessLinkId = accessLink?.Id;
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
                    "Patient intake authentication audit correlation id is required.",
                    nameof(correlationId));
            }

            var normalized = correlationId.Trim();
            if (normalized.Length > CorrelationIdMaxLength)
            {
                throw new ArgumentException(
                    $"Patient intake authentication audit correlation id cannot exceed {CorrelationIdMaxLength} characters.",
                    nameof(correlationId));
            }

            return normalized;
        }
    }
}
