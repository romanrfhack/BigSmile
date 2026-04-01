using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace BigSmile.UnitTests.Middleware
{
    // Test logger that captures log entries
    public class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> Logs { get; } = new List<LogEntry>();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Logs.Add(new LogEntry
            {
                LogLevel = logLevel,
                Message = formatter(state, exception),
                Exception = exception
            });
        }

        public class LogEntry
        {
            public LogLevel LogLevel { get; set; }
            public string? Message { get; set; }
            public Exception? Exception { get; set; }
        }
    }

    public class TenantResolutionMiddlewareTests
    {
        private readonly Mock<IHostEnvironment> _mockEnv;
        private readonly TestLogger<TenantResolutionMiddleware> _testLogger;
        private readonly TenantContext _tenantContext;

        public TenantResolutionMiddlewareTests()
        {
            _mockEnv = new Mock<IHostEnvironment>();
            _testLogger = new TestLogger<TenantResolutionMiddleware>();
            _tenantContext = new TenantContext();
        }

        private HttpContext CreateHttpContext(bool isDevelopment, string? tenantHeader = null, string? branchHeader = null, ClaimsPrincipal? user = null)
        {
            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var headers = new HeaderDictionary();

            if (tenantHeader != null)
                headers["X-Tenant-Id"] = tenantHeader;
            if (branchHeader != null)
                headers["X-Branch-Id"] = branchHeader;

            mockRequest.Setup(r => r.Headers).Returns(headers);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);

            // Setup environment
            _mockEnv.Setup(e => e.EnvironmentName).Returns(isDevelopment ? "Development" : "Production");

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(sp => sp.GetService(typeof(IHostEnvironment))).Returns(_mockEnv.Object);
            serviceProvider.Setup(sp => sp.GetService(typeof(ILogger<TenantResolutionMiddleware>))).Returns(_testLogger);

            mockHttpContext.Setup(c => c.RequestServices).Returns(serviceProvider.Object);
            
            // Setup user (ClaimsPrincipal)
            mockHttpContext.Setup(c => c.User).Returns(user ?? new ClaimsPrincipal());
            
            return mockHttpContext.Object;
        }

        [Fact]
        public async Task InvokeAsync_Development_WithValidGuidHeaders_SetsTenantAndBranch()
        {
            // Arrange
            var httpContext = CreateHttpContext(isDevelopment: true,
                tenantHeader: "12345678-1234-1234-1234-123456789abc",
                branchHeader: "87654321-4321-4321-4321-210987654321");
            var middleware = new TenantResolutionMiddleware(Next);

            // Act
            await middleware.InvokeAsync(httpContext, _tenantContext);

            // Assert
            Assert.Equal("12345678-1234-1234-1234-123456789abc", _tenantContext.GetTenantId());
            Assert.Equal("87654321-4321-4321-4321-210987654321", _tenantContext.GetBranchId());
            // Should log debug messages for header resolution
            Assert.Contains(_testLogger.Logs, log => log.LogLevel == LogLevel.Debug && log.Message!.Contains("Tenant resolved from header"));
            Assert.Contains(_testLogger.Logs, log => log.LogLevel == LogLevel.Debug && log.Message!.Contains("Branch resolved from header"));
        }

        [Fact]
        public async Task InvokeAsync_Development_WithInvalidGuidHeaders_LogsWarningAndIgnores()
        {
            // Arrange
            var httpContext = CreateHttpContext(isDevelopment: true,
                tenantHeader: "not-a-guid",
                branchHeader: "also-not-a-guid");
            var middleware = new TenantResolutionMiddleware(Next);

            // Act
            await middleware.InvokeAsync(httpContext, _tenantContext);

            // Assert
            Assert.Null(_tenantContext.GetTenantId());
            Assert.Null(_tenantContext.GetBranchId());
            Assert.Contains(_testLogger.Logs, log => log.LogLevel == LogLevel.Warning && log.Message!.Contains("Invalid tenant ID format"));
            Assert.Contains(_testLogger.Logs, log => log.LogLevel == LogLevel.Warning && log.Message!.Contains("Invalid branch ID format"));
        }

        [Fact]
        public async Task InvokeAsync_Development_NoHeaders_DoesNothing()
        {
            // Arrange
            var httpContext = CreateHttpContext(isDevelopment: true);
            var middleware = new TenantResolutionMiddleware(Next);

            // Act
            await middleware.InvokeAsync(httpContext, _tenantContext);

            // Assert
            Assert.Null(_tenantContext.GetTenantId());
            Assert.Null(_tenantContext.GetBranchId());
            Assert.Empty(_testLogger.Logs);
        }

        [Fact]
        public async Task InvokeAsync_NonDevelopment_WithHeaders_LogsErrorAndIgnoresHeaders()
        {
            // Arrange
            var httpContext = CreateHttpContext(isDevelopment: false,
                tenantHeader: "12345678-1234-1234-1234-123456789abc",
                branchHeader: "87654321-4321-4321-4321-210987654321");
            var middleware = new TenantResolutionMiddleware(Next);

            // Act
            await middleware.InvokeAsync(httpContext, _tenantContext);

            // Assert
            Assert.Null(_tenantContext.GetTenantId());
            Assert.Null(_tenantContext.GetBranchId());
            Assert.Contains(_testLogger.Logs, log => log.LogLevel == LogLevel.Error && log.Message!.Contains("Header-based tenant resolution is not allowed"));
        }

        [Fact]
        public async Task InvokeAsync_NonDevelopment_NoHeaders_DoesNothing()
        {
            // Arrange
            var httpContext = CreateHttpContext(isDevelopment: false);
            var middleware = new TenantResolutionMiddleware(Next);

            // Act
            await middleware.InvokeAsync(httpContext, _tenantContext);

            // Assert
            Assert.Null(_tenantContext.GetTenantId());
            Assert.Null(_tenantContext.GetBranchId());
            Assert.Empty(_testLogger.Logs);
        }

        private static Task Next(HttpContext context)
        {
            return Task.CompletedTask;
        }
    }
}