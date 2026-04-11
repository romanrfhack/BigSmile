namespace BigSmile.Application.Features.Scheduling.Dtos
{
    public sealed record AppointmentSummaryDto(
        Guid Id,
        Guid BranchId,
        Guid PatientId,
        string PatientFullName,
        DateTime StartsAt,
        DateTime EndsAt,
        string Status,
        string? Notes,
        string? CancellationReason);

    public sealed record CalendarDayDto(
        DateOnly Date,
        IReadOnlyList<AppointmentSummaryDto> Appointments);

    public sealed record CalendarViewDto(
        Guid BranchId,
        DateOnly StartDate,
        int Days,
        IReadOnlyList<CalendarDayDto> CalendarDays);
}
