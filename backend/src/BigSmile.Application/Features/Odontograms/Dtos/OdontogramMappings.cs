using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Odontograms.Dtos
{
    internal static class OdontogramMappings
    {
        public static OdontogramDetailDto ToDetailDto(this Odontogram odontogram)
        {
            return new OdontogramDetailDto(
                odontogram.Id,
                odontogram.PatientId,
                odontogram.Teeth
                    .OrderBy(tooth => tooth.ToothCode, StringComparer.Ordinal)
                    .Select(tooth => new OdontogramToothStateDto(
                        tooth.ToothCode,
                        tooth.Status.ToString(),
                        tooth.UpdatedAtUtc,
                        tooth.UpdatedByUserId))
                    .ToList(),
                odontogram.CreatedAtUtc,
                odontogram.CreatedByUserId,
                odontogram.LastUpdatedAtUtc,
                odontogram.LastUpdatedByUserId);
        }
    }
}
