using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public class Appointment : Entity<Guid>, ITenantOwnedEntity
    {
        private const int NotesMaxLength = 1000;
        private const int CancellationReasonMaxLength = 500;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public string? Notes { get; private set; }
        public AppointmentStatus Status { get; private set; } = AppointmentStatus.Scheduled;
        public AppointmentConfirmationStatus ConfirmationStatus { get; private set; } = AppointmentConfirmationStatus.Pending;
        public DateTime? ConfirmedAtUtc { get; private set; }
        public Guid? ConfirmedByUserId { get; private set; }
        public bool ReminderRequired { get; private set; }
        public AppointmentReminderChannel? ReminderChannel { get; private set; }
        public DateTime? ReminderDueAtUtc { get; private set; }
        public DateTime? ReminderCompletedAtUtc { get; private set; }
        public Guid? ReminderCompletedByUserId { get; private set; }
        public DateTime? ReminderUpdatedAtUtc { get; private set; }
        public Guid? ReminderUpdatedByUserId { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public string? CancellationReason { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }
        public ICollection<AppointmentReminderLogEntry> ReminderLogEntries { get; private set; } = new List<AppointmentReminderLogEntry>();

        protected Appointment()
        {
        }

        public Appointment(
            Guid tenantId,
            Guid branchId,
            Guid patientId,
            DateTime startsAt,
            DateTime endsAt,
            string? notes = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Appointment tenant ownership is required.", nameof(tenantId));
            }

            if (branchId == Guid.Empty)
            {
                throw new ArgumentException("Appointment branch is required.", nameof(branchId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Appointment patient is required.", nameof(patientId));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            BranchId = branchId;
            PatientId = patientId;

            SetSchedule(startsAt, endsAt);
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            ConfirmationStatus = AppointmentConfirmationStatus.Pending;
        }

        public void Update(Guid patientId, DateTime startsAt, DateTime endsAt, string? notes)
        {
            EnsureStatusAllows("modified");

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Appointment patient is required.", nameof(patientId));
            }

            PatientId = patientId;
            SetSchedule(startsAt, endsAt);
            Notes = NormalizeOptional(notes, nameof(notes), NotesMaxLength);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reschedule(DateTime startsAt, DateTime endsAt)
        {
            EnsureStatusAllows("rescheduled");
            SetSchedule(startsAt, endsAt);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string? cancellationReason = null)
        {
            if (Status == AppointmentStatus.Cancelled)
            {
                return;
            }

            EnsureStatusAllows("cancelled");
            Status = AppointmentStatus.Cancelled;
            CancellationReason = NormalizeOptional(
                cancellationReason,
                nameof(cancellationReason),
                CancellationReasonMaxLength);
            CancelledAt = DateTime.UtcNow;
            UpdatedAt = CancelledAt;
        }

        public void MarkAttended()
        {
            EnsureStatusAllows("marked as attended");
            Status = AppointmentStatus.Attended;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkNoShow()
        {
            EnsureStatusAllows("marked as no-show");
            Status = AppointmentStatus.NoShow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Confirm(Guid actorUserId)
        {
            EnsureActor(actorUserId);
            EnsureConfirmationCanChange();

            var confirmedAtUtc = DateTime.UtcNow;
            ConfirmationStatus = AppointmentConfirmationStatus.Confirmed;
            ConfirmedAtUtc = confirmedAtUtc;
            ConfirmedByUserId = actorUserId;
            UpdatedAt = confirmedAtUtc;
        }

        public void MarkConfirmationPending(Guid actorUserId)
        {
            EnsureActor(actorUserId);
            EnsureConfirmationCanChange();

            if (ConfirmationStatus == AppointmentConfirmationStatus.Pending)
            {
                return;
            }

            ConfirmationStatus = AppointmentConfirmationStatus.Pending;
            ConfirmedAtUtc = null;
            ConfirmedByUserId = null;
            UpdatedAt = DateTime.UtcNow;
        }

        public void ConfigureManualReminder(
            AppointmentReminderChannel channel,
            DateTime dueAtUtc,
            Guid actorUserId)
        {
            EnsureActor(actorUserId);
            EnsureManualReminderCanBeConfigured();

            if (!Enum.IsDefined(channel))
            {
                throw new ArgumentException(
                    "Appointment reminder channel must be one of: Phone, WhatsApp, Email or Other.",
                    nameof(channel));
            }

            if (dueAtUtc == default)
            {
                throw new ArgumentException("Manual reminder due date/time is required.", nameof(dueAtUtc));
            }

            var updatedAtUtc = DateTime.UtcNow;
            ReminderRequired = true;
            ReminderChannel = channel;
            ReminderDueAtUtc = dueAtUtc;
            ReminderCompletedAtUtc = null;
            ReminderCompletedByUserId = null;
            ReminderUpdatedAtUtc = updatedAtUtc;
            ReminderUpdatedByUserId = actorUserId;
            UpdatedAt = updatedAtUtc;
        }

        public void ClearManualReminder(Guid actorUserId)
        {
            EnsureActor(actorUserId);

            var updatedAtUtc = DateTime.UtcNow;
            ReminderRequired = false;
            ReminderChannel = null;
            ReminderDueAtUtc = null;
            ReminderCompletedAtUtc = null;
            ReminderCompletedByUserId = null;
            ReminderUpdatedAtUtc = updatedAtUtc;
            ReminderUpdatedByUserId = actorUserId;
            UpdatedAt = updatedAtUtc;
        }

        public void CompleteManualReminder(Guid actorUserId)
        {
            EnsureActor(actorUserId);

            if (!ReminderRequired)
            {
                throw new InvalidOperationException("Manual reminder completion requires an active reminder intention.");
            }

            if (ReminderCompletedAtUtc.HasValue)
            {
                return;
            }

            var completedAtUtc = DateTime.UtcNow;
            ReminderCompletedAtUtc = completedAtUtc;
            ReminderCompletedByUserId = actorUserId;
            ReminderUpdatedAtUtc = completedAtUtc;
            ReminderUpdatedByUserId = actorUserId;
            UpdatedAt = completedAtUtc;
        }

        public AppointmentReminderLogEntry AddReminderLogEntry(
            AppointmentReminderChannel channel,
            AppointmentReminderOutcome outcome,
            string? notes,
            Guid createdByUserId)
        {
            EnsureActor(createdByUserId);

            var entry = new AppointmentReminderLogEntry(
                TenantId,
                Id,
                channel,
                outcome,
                notes,
                createdByUserId);

            ReminderLogEntries.Add(entry);
            return entry;
        }

        private void EnsureStatusAllows(string action)
        {
            if (Status == AppointmentStatus.Scheduled)
            {
                return;
            }

            var statusName = Status switch
            {
                AppointmentStatus.NoShow => "No-show",
                _ => Status.ToString()
            };

            throw new InvalidOperationException($"{statusName} appointments cannot be {action}.");
        }

        private void EnsureConfirmationCanChange()
        {
            if (Status == AppointmentStatus.Scheduled)
            {
                return;
            }

            var statusName = Status switch
            {
                AppointmentStatus.NoShow => "No-show",
                _ => Status.ToString()
            };

            throw new InvalidOperationException($"{statusName} appointments cannot have confirmation changed.");
        }

        private void EnsureManualReminderCanBeConfigured()
        {
            if (Status == AppointmentStatus.Scheduled)
            {
                return;
            }

            var statusName = Status switch
            {
                AppointmentStatus.NoShow => "No-show",
                _ => Status.ToString()
            };

            throw new InvalidOperationException($"{statusName} appointments cannot have new manual reminders configured.");
        }

        private static void EnsureActor(Guid actorUserId)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Appointment mutation actor is required.", nameof(actorUserId));
            }
        }

        private void SetSchedule(DateTime startsAt, DateTime endsAt)
        {
            if (startsAt == default)
            {
                throw new ArgumentException("Appointment start time is required.", nameof(startsAt));
            }

            if (endsAt == default)
            {
                throw new ArgumentException("Appointment end time is required.", nameof(endsAt));
            }

            if (endsAt <= startsAt)
            {
                throw new ArgumentException("Appointment end time must be after the start time.", nameof(endsAt));
            }

            StartsAt = startsAt;
            EndsAt = endsAt;
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
