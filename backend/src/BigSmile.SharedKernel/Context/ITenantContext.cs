namespace BigSmile.SharedKernel.Context
{
    public interface ITenantContext
    {
        /// <summary>
        /// Gets the current tenant identifier (if any).
        /// </summary>
        /// <returns>The tenant identifier, or null if not set.</returns>
        string? GetTenantId();

        /// <summary>
        /// Gets the current branch identifier (if any).
        /// </summary>
        /// <returns>The branch identifier, or null if not set.</returns>
        string? GetBranchId();
    }
}