using BigSmile.SharedKernel.Authorization;

namespace BigSmile.SharedKernel.Context
{
    public interface ITenantContext
    {
        /// <summary>
        /// Gets the current authenticated user identifier (if any).
        /// </summary>
        /// <returns>The current user identifier, or null if not set.</returns>
        string? GetUserId();

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

        /// <summary>
        /// Gets the current access scope for the request.
        /// </summary>
        /// <returns>The resolved access scope.</returns>
        AccessScope GetAccessScope();

        /// <summary>
        /// Indicates whether the current request is authenticated.
        /// </summary>
        /// <returns>True when the request has an authenticated identity.</returns>
        bool IsAuthenticated();

        /// <summary>
        /// Indicates whether a platform override has been explicitly enabled for the request.
        /// </summary>
        /// <returns>True when platform override is active.</returns>
        bool HasPlatformOverride();
    }
}
