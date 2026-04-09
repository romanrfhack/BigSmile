namespace BigSmile.SharedKernel.Multitenancy
{
    public interface ITenantOwnedEntity
    {
        Guid TenantId { get; }
    }
}
