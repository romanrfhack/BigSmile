using BigSmile.SharedKernel.Authorization;
using BigSmile.Infrastructure.Context;
using Xunit;

namespace BigSmile.UnitTests.Context
{
    public class TenantContextTests
    {
        [Fact]
        public void SetTenantId_GetTenantId_ReturnsSameValue()
        {
            // Arrange
            var context = new TenantContext();
            var expectedTenantId = "tenant-123";

            // Act
            context.SetTenantId(expectedTenantId);
            var actual = context.GetTenantId();

            // Assert
            Assert.Equal(expectedTenantId, actual);
        }

        [Fact]
        public void SetBranchId_GetBranchId_ReturnsSameValue()
        {
            // Arrange
            var context = new TenantContext();
            var expectedBranchId = "branch-456";

            // Act
            context.SetBranchId(expectedBranchId);
            var actual = context.GetBranchId();

            // Assert
            Assert.Equal(expectedBranchId, actual);
        }

        [Fact]
        public void GetTenantId_Initially_ReturnsNull()
        {
            // Arrange
            var context = new TenantContext();

            // Act
            var userId = context.GetUserId();
            var tenantId = context.GetTenantId();
            var branchId = context.GetBranchId();
            var accessScope = context.GetAccessScope();
            var isAuthenticated = context.IsAuthenticated();
            var hasPlatformOverride = context.HasPlatformOverride();

            // Assert
            Assert.Null(userId);
            Assert.Null(tenantId);
            Assert.Null(branchId);
            Assert.Equal(AccessScope.Anonymous, accessScope);
            Assert.False(isAuthenticated);
            Assert.False(hasPlatformOverride);
        }

        [Fact]
        public void SetTenantId_Null_AllowsNull()
        {
            // Arrange
            var context = new TenantContext();
            context.SetTenantId("some-tenant");
            context.SetBranchId("some-branch");

            // Act
            context.SetTenantId(null!);
            context.SetBranchId(null!);

            // Assert
            Assert.Null(context.GetTenantId());
            Assert.Null(context.GetBranchId());
        }

        [Fact]
        public void SetRequestContext_PopulatesResolvedAccessContext()
        {
            var context = new TenantContext();

            context.SetRequestContext(
                userId: "user-1",
                accessScope: AccessScope.Branch,
                isAuthenticated: true,
                tenantId: "tenant-1",
                branchId: "branch-1");

            Assert.Equal("user-1", context.GetUserId());
            Assert.Equal("tenant-1", context.GetTenantId());
            Assert.Equal("branch-1", context.GetBranchId());
            Assert.Equal(AccessScope.Branch, context.GetAccessScope());
            Assert.True(context.IsAuthenticated());
        }

        [Fact]
        public void EnablePlatformOverride_SetsOverrideFlag()
        {
            var context = new TenantContext();
            context.SetRequestContext("user-1", AccessScope.Platform, isAuthenticated: true);

            context.EnablePlatformOverride();

            Assert.True(context.HasPlatformOverride());
        }
    }
}
