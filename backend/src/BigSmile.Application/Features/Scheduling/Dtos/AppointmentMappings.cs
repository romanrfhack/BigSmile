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
                appointment.Notes,
                appointment.CancellationReason);
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
    }
}
