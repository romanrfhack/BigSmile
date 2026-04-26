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
        string ConfirmationStatus,
        DateTime? ConfirmedAtUtc,
        Guid? ConfirmedByUserId,
        string? Notes,
        string? CancellationReason);

    public sealed record AppointmentBlockSummaryDto(
        Guid Id,
        Guid BranchId,
        DateTime StartsAt,
        DateTime EndsAt,
        string? Label);

    public sealed record AppointmentReminderLogEntryDto(
        Guid Id,
        Guid AppointmentId,
        string Channel,
        string Outcome,
        string? Notes,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId);

    public sealed record CalendarDayDto(
        DateOnly Date,
        IReadOnlyList<AppointmentSummaryDto> Appointments,
        IReadOnlyList<AppointmentBlockSummaryDto> BlockedSlots);

    public sealed record CalendarViewDto(
        Guid BranchId,
        DateOnly StartDate,
        int Days,
        IReadOnlyList<CalendarDayDto> CalendarDays);
}
