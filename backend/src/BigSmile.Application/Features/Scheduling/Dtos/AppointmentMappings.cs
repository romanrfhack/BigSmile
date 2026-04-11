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
                appointment.Notes,
                appointment.CancellationReason);
        }
    }
}
