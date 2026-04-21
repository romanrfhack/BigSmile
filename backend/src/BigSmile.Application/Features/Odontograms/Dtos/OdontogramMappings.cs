using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.Odontograms.Dtos
{
    internal static class OdontogramMappings
    {
        public static OdontogramDetailDto ToDetailDto(this Odontogram odontogram)
        {
            var surfacesByTooth = odontogram.Surfaces
                .GroupBy(surface => surface.ToothCode, StringComparer.Ordinal)
                .ToDictionary(
                    grouping => grouping.Key,
                    grouping => (IReadOnlyList<OdontogramSurfaceStateDto>)grouping
                        .OrderBy(surface => OdontogramSurfaceState.GetSurfaceSortOrder(surface.SurfaceCode))
                        .Select(surface => new OdontogramSurfaceStateDto(
                            surface.SurfaceCode,
                            surface.Status.ToString(),
                            surface.UpdatedAtUtc,
                            surface.UpdatedByUserId))
                        .ToList(),
                    StringComparer.Ordinal);

            return new OdontogramDetailDto(
                odontogram.Id,
                odontogram.PatientId,
                odontogram.Teeth
                    .OrderBy(tooth => tooth.ToothCode, StringComparer.Ordinal)
                    .Select(tooth => new OdontogramToothStateDto(
                        tooth.ToothCode,
                        tooth.Status.ToString(),
                        tooth.UpdatedAtUtc,
                        tooth.UpdatedByUserId,
                        surfacesByTooth.TryGetValue(tooth.ToothCode, out var surfaces)
                            ? surfaces
                            : []))
                    .ToList(),
                odontogram.CreatedAtUtc,
                odontogram.CreatedByUserId,
                odontogram.LastUpdatedAtUtc,
                odontogram.LastUpdatedByUserId);
        }
    }
}
