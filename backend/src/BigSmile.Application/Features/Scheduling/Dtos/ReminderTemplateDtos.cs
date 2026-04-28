namespace BigSmile.Application.Features.Scheduling.Dtos
{
    public sealed record ReminderTemplateDto(
        Guid Id,
        string Name,
        string Body,
        bool IsActive,
        DateTime CreatedAtUtc,
        Guid CreatedByUserId,
        DateTime? UpdatedAtUtc,
        Guid? UpdatedByUserId,
        DateTime? DeactivatedAtUtc,
        Guid? DeactivatedByUserId);

    public sealed record ReminderTemplatePreviewDto(
        Guid TemplateId,
        Guid AppointmentId,
        string RenderedBody,
        IReadOnlyList<string> UnknownPlaceholders);
}
