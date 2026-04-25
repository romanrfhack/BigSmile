using BigSmile.Api.Authorization;
using BigSmile.Api.Controllers;
using Microsoft.AspNetCore.Authorization;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class BranchAuthorizationPolicyTests
    {
        [Fact]
        public void GetAccessibleBranches_UsesCurrentTenantReadPolicy()
        {
            var method = typeof(BranchesController).GetMethod(nameof(BranchesController.GetAccessible));

            Assert.NotNull(method);
            var authorizeAttribute = Assert.Single(
                method!.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false).Cast<AuthorizeAttribute>());
            Assert.Equal(AuthorizationPolicies.CurrentTenantRead, authorizeAttribute.Policy);
        }
    }
}
