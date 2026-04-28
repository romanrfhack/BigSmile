using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class AppointmentReminderLogEntry : Entity<Guid>, ITenantOwnedEntity
    {
        public const int NotesMaxLength = 500;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid AppointmentId { get; private set; }
        public Appointment Appointment { get; private set; } = null!;

        public AppointmentReminderChannel Channel { get; private set; }
        public AppointmentReminderOutcome Outcome { get; private set; }
        public string? Notes { get; private set; }
        public Guid? ReminderTemplateId { get; private set; }
        public ReminderTemplate? ReminderTemplate { get; private set; }
        public string? ReminderTemplateNameSnapshot { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private AppointmentReminderLogEntry()
        {
        }

        public AppointmentReminderLogEntry(
            Guid tenantId,
            Guid appointmentId,
            AppointmentReminderChannel channel,
            AppointmentReminderOutcome outcome,
            string? notes,
            Guid createdByUserId,
            DateTime? createdAtUtc = null)
            : this(
                tenantId,
                appointmentId,
                channel,
                outcome,
                notes,
                createdByUserId,
                reminderTemplateId: null,
                reminderTemplateNameSnapshot: null,
                createdAtUtc)
        {
        }

        public AppointmentReminderLogEntry(
            Guid tenantId,
            Guid appointmentId,
            AppointmentReminderChannel channel,
            AppointmentReminderOutcome outcome,
            string? notes,
            Guid createdByUserId,
            Guid? reminderTemplateId,
            string? reminderTemplateNameSnapshot,
            DateTime? createdAtUtc = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Appointment reminder log tenant ownership is required.", nameof(tenantId));
            }

            if (appointmentId == Guid.Empty)
            {
                throw new ArgumentException("Appointment reference is required.", nameof(appointmentId));
            }

            if (!Enum.IsDefined(channel))
            {
                throw new ArgumentException("Appointment reminder channel must be one of: Phone, WhatsApp, Email or Other.", nameof(channel));
            }

            if (!Enum.IsDefined(outcome))
            {
                throw new ArgumentException("Appointment reminder outcome must be one of: Reached, NoAnswer or LeftMessage.", nameof(outcome));
            }

            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Appointment reminder log author is required.", nameof(createdByUserId));
            }

            if (createdAtUtc == default(DateTime))
            {
                throw new ArgumentException("Appointment reminder log creation time is required.", nameof(createdAtUtc));
            }

            if (reminderTemplateId == Guid.Empty)
            {
                throw new ArgumentException("Reminder template reference must be a non-empty identifier when provided.", nameof(reminderTemplateId));
            }

            var normalizedTemplateNameSnapshot = NormalizeOptional(
                reminderTemplateNameSnapshot,
                nameof(reminderTemplateNameSnapshot),
                ReminderTemplate.NameMaxLength);
            if (reminderTemplateId.HasValue && normalizedTemplateNameSnapshot == null)
            {
                throw new ArgumentException(
                    "Reminder template name snapshot is required when a reminder template reference is provided.",
                    nameof(reminderTemplateNameSnapshot));
            }

            if (!reminderTemplateId.HasValue && normalizedTemplateNameSnapshot != null)
            {
                throw new ArgumentException(
                    "Reminder template name snapshot cannot be set without a reminder template reference.",
                    nameof(reminderTemplateNameSnapshot));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            AppointmentId = appointmentId;
            Channel = channel;
            Outcome = outcome;
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            ReminderTemplateId = reminderTemplateId;
            ReminderTemplateNameSnapshot = normalizedTemplateNameSnapshot;
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow;
        }

        private static string? NormalizeOptional(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
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
