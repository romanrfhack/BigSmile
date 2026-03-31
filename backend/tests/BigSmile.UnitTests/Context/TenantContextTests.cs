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
            var tenantId = context.GetTenantId();
            var branchId = context.GetBranchId();

            // Assert
            Assert.Null(tenantId);
            Assert.Null(branchId);
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
    }
}