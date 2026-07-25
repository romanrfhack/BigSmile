using System.Reflection;
using BigSmile.Api.Authorization;
using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Commands;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos;
using BigSmile.Application.Features.PatientIntakeAccessLinks.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAccessLinksControllerTests
    {
        private static readonly DateTime CreatedAtUtc =
            new(2026, 7, 25, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public async Task Issue_ReturnsBootstrapTokenOnceWithNoStoreHeaders()
        {
            var branchId = Guid.NewGuid();
            var issued = new IssuedPatientIntakeAccessLinkDto(
                Guid.NewGuid(),
                branchId,
                "Main",
                "one-time-bootstrap-token",
                CreatedAtUtc,
                CreatedAtUtc.AddMinutes(30));
            var commandService = new Mock<IPatientIntakeAccessLinkCommandService>();
            commandService
                .Setup(service => service.IssueAsync(
                    branchId,
                    "trace-link-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(issued);
            var controller = CreateController(
                commandService.Object,
                Mock.Of<IPatientIntakeAccessLinkQueryService>());

            var result = await controller.Issue(
                new PatientIntakeAccessLinksController.IssuePatientIntakeAccessLinkRequest
                {
                    BranchId = branchId
                });

            var created = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Same(issued, created.Value);
            Assert.Equal("one-time-bootstrap-token", issued.BootstrapToken);
            AssertNoStore(controller.Response.Headers);
        }

        [Fact]
        public async Task List_ReturnsMetadataWithoutTokenMaterial()
        {
            var summaries = new[]
            {
                new PatientIntakeAccessLinkSummaryDto(
                    Guid.NewGuid(),
                    null,
                    null,
                    "Active",
                    CreatedAtUtc,
                    CreatedAtUtc.AddMinutes(30),
                    null,
                    null,
                    true)
            };
            var queryService = new Mock<IPatientIntakeAccessLinkQueryService>();
            queryService
                .Setup(service => service.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(summaries);
            var controller = CreateController(
                Mock.Of<IPatientIntakeAccessLinkCommandService>(),
                queryService.Object);

            var result = await controller.List();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(summaries, ok.Value);
            Assert.DoesNotContain(
                typeof(PatientIntakeAccessLinkSummaryDto).GetProperties(),
                property => property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase) ||
                            property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
            AssertNoStore(controller.Response.Headers);
        }

        [Fact]
        public async Task Revoke_MapsMissingAndSuccessWithoutDisclosingToken()
        {
            var accessLinkId = Guid.NewGuid();
            var commandService = new Mock<IPatientIntakeAccessLinkCommandService>();
            commandService
                .SetupSequence(service => service.RevokeAsync(
                    accessLinkId,
                    "trace-link-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false)
                .ReturnsAsync(true);
            var controller = CreateController(
                commandService.Object,
                Mock.Of<IPatientIntakeAccessLinkQueryService>());

            var missing = await controller.Revoke(accessLinkId);
            var revoked = await controller.Revoke(accessLinkId);

            Assert.IsType<NotFoundResult>(missing);
            Assert.IsType<NoContentResult>(revoked);
            AssertNoStore(controller.Response.Headers);
        }

        [Fact]
        public void Controller_UsesStaffOnlyRouteAndRequestHasNoTenantOrTokenFields()
        {
            var route = typeof(PatientIntakeAccessLinksController)
                .GetCustomAttribute<RouteAttribute>();
            var authorize = typeof(PatientIntakeAccessLinksController)
                .GetCustomAttribute<AuthorizeAttribute>();

            Assert.Equal("api/patient-intake-links", route!.Template);
            Assert.DoesNotStartWith(
                "api/patient-portal/",
                route.Template!,
                StringComparison.OrdinalIgnoreCase);
            Assert.Equal(
                AuthorizationPolicies.PatientIntakeAccessLinkManage,
                authorize!.Policy);

            var requestProperties = typeof(
                    PatientIntakeAccessLinksController.IssuePatientIntakeAccessLinkRequest)
                .GetProperties()
                .Select(property => property.Name)
                .ToArray();
            Assert.Equal(new[] { "BranchId" }, requestProperties);
        }

        private static PatientIntakeAccessLinksController CreateController(
            IPatientIntakeAccessLinkCommandService commandService,
            IPatientIntakeAccessLinkQueryService queryService)
        {
            return new PatientIntakeAccessLinksController(
                commandService,
                queryService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        TraceIdentifier = "trace-link-1"
                    }
                }
            };
        }

        private static void AssertNoStore(IHeaderDictionary headers)
        {
            Assert.Equal("no-store", headers.CacheControl.ToString());
            Assert.Equal("no-cache", headers.Pragma.ToString());
            Assert.Equal("0", headers.Expires.ToString());
        }
    }
}
