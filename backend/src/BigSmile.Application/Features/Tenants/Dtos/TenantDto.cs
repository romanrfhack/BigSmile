namespace BigSmile.Application.Features.Tenants.Dtos
{
    public record TenantDto(
        Guid Id,
        string Name,
        string? Subdomain,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}