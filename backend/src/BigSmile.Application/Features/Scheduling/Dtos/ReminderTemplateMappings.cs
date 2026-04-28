using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Scheduling.Dtos
{
    internal static class ReminderTemplateMappings
    {
        public static ReminderTemplateDto ToDto(this ReminderTemplate template)
        {
            return new ReminderTemplateDto(
                template.Id,
                template.Name,
                template.Body,
                template.IsActive,
                template.CreatedAtUtc,
                template.CreatedByUserId,
                template.UpdatedAtUtc,
                template.UpdatedByUserId,
                template.DeactivatedAtUtc,
                template.DeactivatedByUserId);
        }
    }
}
