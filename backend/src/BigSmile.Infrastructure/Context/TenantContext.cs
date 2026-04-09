using BigSmile.SharedKernel.Authorization;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Infrastructure.Context
{
    public class TenantContext : ITenantContext
    {
        private string? _userId;
        private string? _tenantId;
        private string? _branchId;
        private AccessScope _accessScope = AccessScope.Anonymous;
        private bool _isAuthenticated;
        private bool _platformOverrideEnabled;

        public string? GetUserId() => _userId;
        public string? GetTenantId() => _tenantId;
        public string? GetBranchId() => _branchId;
        public AccessScope GetAccessScope() => _accessScope;
        public bool IsAuthenticated() => _isAuthenticated;
        public bool HasPlatformOverride() => _platformOverrideEnabled;

        public void SetUserId(string? userId) => _userId = Normalize(userId);
        public void SetTenantId(string? tenantId) => _tenantId = Normalize(tenantId);
        public void SetBranchId(string? branchId) => _branchId = Normalize(branchId);
        public void SetAccessScope(AccessScope accessScope) => _accessScope = accessScope;
        public void SetIsAuthenticated(bool isAuthenticated) => _isAuthenticated = isAuthenticated;

        public void SetRequestContext(
            string? userId,
            AccessScope accessScope,
            bool isAuthenticated,
            string? tenantId = null,
            string? branchId = null)
        {
            _platformOverrideEnabled = false;
            SetUserId(userId);
            SetTenantId(tenantId);
            SetBranchId(branchId);
            SetAccessScope(accessScope);
            SetIsAuthenticated(isAuthenticated);
        }

        public void EnablePlatformOverride() => _platformOverrideEnabled = true;

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
