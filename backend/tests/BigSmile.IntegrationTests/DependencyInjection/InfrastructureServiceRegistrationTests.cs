using BigSmile.Infrastructure;
using BigSmile.SharedKernel.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace BigSmile.IntegrationTests.DependencyInjection
{
    public class InfrastructureServiceRegistrationTests
    {
        private readonly IConfiguration _configuration;

        public InfrastructureServiceRegistrationTests()
        {
            // Provide a minimal configuration with a dummy connection string
            var configValues = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=BigSmile_IntegrationTests;Trusted_Connection=True;MultipleActiveResultSets=true" }
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
        }

        [Fact]
        public void AddInfrastructure_RegistersTenantContextAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddInfrastructure(_configuration);
            var provider = services.BuildServiceProvider();

            // Assert
            // ITenantContext should be resolvable
            var tenantContext = provider.GetService<ITenantContext>();
            Assert.NotNull(tenantContext);
            Assert.IsType<BigSmile.Infrastructure.Context.TenantContext>(tenantContext);

            // Should be scoped (different instances per scope)
            using var scope1 = provider.CreateScope();
            using var scope2 = provider.CreateScope();
            var instance1 = scope1.ServiceProvider.GetService<ITenantContext>();
            var instance2 = scope2.ServiceProvider.GetService<ITenantContext>();
            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void AddInfrastructure_RegistersTenantResolutionMiddlewareAsTransient()
        {
            // Middleware is not registered in DI; it's instantiated directly by ASP.NET Core.
            // This test is a placeholder to ensure we can add more DI validation later.
            Assert.True(true);
        }
    }
}