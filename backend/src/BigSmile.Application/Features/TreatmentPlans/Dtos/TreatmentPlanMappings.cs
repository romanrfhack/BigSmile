using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.TreatmentPlans.Dtos
{
    internal static class TreatmentPlanMappings
    {
        public static TreatmentPlanDetailDto ToDetailDto(this TreatmentPlan treatmentPlan)
        {
            return new TreatmentPlanDetailDto(
                treatmentPlan.Id,
                treatmentPlan.PatientId,
                treatmentPlan.Status.ToString(),
                treatmentPlan.Items
                    .OrderBy(item => item.CreatedAtUtc)
                    .ThenBy(item => item.Id)
                    .Select(item => new TreatmentPlanItemDto(
                        item.Id,
                        item.Title,
                        item.Category,
                        item.Quantity,
                        item.Notes,
                        item.ToothCode,
                        item.SurfaceCode,
                        item.CreatedAtUtc,
                        item.CreatedByUserId))
                    .ToList(),
                treatmentPlan.CreatedAtUtc,
                treatmentPlan.CreatedByUserId,
                treatmentPlan.LastUpdatedAtUtc,
                treatmentPlan.LastUpdatedByUserId);
        }
    }
}
