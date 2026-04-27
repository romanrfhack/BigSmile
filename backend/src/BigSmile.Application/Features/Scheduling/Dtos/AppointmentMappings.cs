using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Scheduling.Dtos
{
    internal static class AppointmentMappings
    {
        public static AppointmentSummaryDto ToSummaryDto(this Appointment appointment)
        {
            return new AppointmentSummaryDto(
                appointment.Id,
                appointment.BranchId,
                appointment.PatientId,
                appointment.Patient.FullName,
                appointment.StartsAt,
                appointment.EndsAt,
                appointment.Status.ToString(),
                appointment.ConfirmationStatus.ToString(),
                appointment.ConfirmedAtUtc,
                appointment.ConfirmedByUserId,
                appointment.ReminderRequired,
                appointment.ReminderChannel?.ToString(),
                appointment.ReminderDueAtUtc,
                appointment.ReminderCompletedAtUtc,
                appointment.ReminderCompletedByUserId,
                appointment.ReminderUpdatedAtUtc,
                appointment.ReminderUpdatedByUserId,
                appointment.Notes,
                appointment.CancellationReason);
        }

        public static AppointmentReminderWorkItemDto ToReminderWorkItemDto(this Appointment appointment, DateTime utcNow)
        {
            return new AppointmentReminderWorkItemDto(
                appointment.Id,
                appointment.BranchId,
                appointment.PatientId,
                appointment.Patient.FullName,
                appointment.StartsAt,
                appointment.Status.ToString(),
                appointment.ConfirmationStatus.ToString(),
                appointment.ReminderChannel?.ToString(),
                appointment.ReminderDueAtUtc,
                GetReminderState(appointment, utcNow),
                appointment.ReminderCompletedAtUtc,
                appointment.ReminderCompletedByUserId);
        }

        public static AppointmentBlockSummaryDto ToSummaryDto(this AppointmentBlock appointmentBlock)
        {
            return new AppointmentBlockSummaryDto(
                appointmentBlock.Id,
                appointmentBlock.BranchId,
                appointmentBlock.StartsAt,
                appointmentBlock.EndsAt,
                appointmentBlock.Label);
        }

        public static AppointmentReminderLogEntryDto ToDto(this AppointmentReminderLogEntry entry)
        {
            return new AppointmentReminderLogEntryDto(
                entry.Id,
                entry.AppointmentId,
                entry.Channel.ToString(),
                entry.Outcome.ToString(),
                entry.Notes,
                entry.CreatedAtUtc,
                entry.CreatedByUserId);
        }

        private static string GetReminderState(Appointment appointment, DateTime utcNow)
        {
            if (!appointment.ReminderRequired)
            {
                return "NotRequired";
            }

            if (appointment.ReminderCompletedAtUtc.HasValue)
            {
                return "Completed";
            }

            return appointment.ReminderDueAtUtc <= utcNow ? "Due" : "Pending";
        }
    }
}
