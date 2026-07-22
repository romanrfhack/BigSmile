namespace BigSmile.Application.Features.Tenants.Dtos
{
    public record TenantDto(
        Guid Id,
        string Name,
        string? Subdomain,
        string TimeZoneId,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
