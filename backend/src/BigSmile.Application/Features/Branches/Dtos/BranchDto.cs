namespace BigSmile.Application.Features.Branches.Dtos
{
    public record BranchDto(
        Guid Id,
        Guid TenantId,
        string Name,
        string? Address,
        bool IsActive,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
