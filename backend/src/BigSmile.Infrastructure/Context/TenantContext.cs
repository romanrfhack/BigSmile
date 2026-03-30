using BigSmile.SharedKernel.Context;

namespace BigSmile.Infrastructure.Context
{
    public class TenantContext : ITenantContext
    {
        private string? _tenantId;
        private string? _branchId;

        public string? GetTenantId() => _tenantId;
        public string? GetBranchId() => _branchId;

        public void SetTenantId(string tenantId) => _tenantId = tenantId;
        public void SetBranchId(string branchId) => _branchId = branchId;
    }
}