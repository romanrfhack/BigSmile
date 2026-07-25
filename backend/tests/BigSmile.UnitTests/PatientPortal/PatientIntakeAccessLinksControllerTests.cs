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
        [Fact]
        public async Task Issue_ReturnsRawTokenOnceWithNoStoreHeaders()
        {
            var branchId = Guid.NewGuid();
            var issued = new IssuedPatientIntakeAccessLinkDto(
                Guid.NewGuid(),
                branchId,
                "NewPatientWaitingRoomRegistration",
                "raw-one-time-token",
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(30));
            var commands = new Mock<IPatientIntakeAccessLinkCommandService>();
            commands
                .Setup(service => service.IssueAsync(
                    branchId,
                    "trace-link-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeAccessLinkIssueResult.Success(issued));
            var controller = CreateController(
                commands.Object,
                Mock.Of<IPatientIntakeAccessLinkQueryService>());

            var result = await controller.Issue(
                new PatientIntakeAccessLinksController.IssuePatientIntakeAccessLinkRequest
                {
                    BranchId = branchId
                });

            var created = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Same(issued, created.Value);
            Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
            Assert.Equal("no-cache", controller.Response.Headers.Pragma.ToString());
            Assert.Equal("0", controller.Response.Headers.Expires.ToString());
        }

        [Fact]
        public async Task List_ReturnsMetadataWithoutTokenMaterial()
        {
            var summary = new PatientIntakeAccessLinkSummaryDto(
                Guid.NewGuid(),
                null,
                "NewPatientWaitingRoomRegistration",
                "Active",
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(30),
                null,
                null);
            var queries = new Mock<IPatientIntakeAccessLinkQueryService>();
            queries
                .Setup(service => service.ListAsync(
                    false,
                    50,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { summary });
            var controller = CreateController(
                Mock.Of<IPatientIntakeAccessLinkCommandService>(),
                queries.Object);

            var result = await controller.List();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var items = Assert.IsAssignableFrom<IReadOnlyList<PatientIntakeAccessLinkSummaryDto>>(ok.Value);
            Assert.Same(summary, Assert.Single(items));
            var properties = typeof(PatientIntakeAccessLinkSummaryDto)
                .GetProperties()
                .Select(property => property.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain("AccessToken", properties);
            Assert.DoesNotContain("Token", properties);
            Assert.DoesNotContain("TokenHash", properties);
        }

        [Fact]
        public async Task Revoke_MapsMissingAndResolvedStatesWithoutOwnershipDisclosure()
        {
            var linkId = Guid.NewGuid();
            var commands = new Mock<IPatientIntakeAccessLinkCommandService>();
            commands
                .SetupSequence(service => service.RevokeAsync(
                    linkId,
                    "trace-link-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeAccessLinkRevokeResult.Failed(
                    PatientIntakeAccessLinkRevokeFailure.Missing))
                .ReturnsAsync(PatientIntakeAccessLinkRevokeResult.Failed(
                    PatientIntakeAccessLinkRevokeFailure.NotActive));
            var controller = CreateController(
                commands.Object,
                Mock.Of<IPatientIntakeAccessLinkQueryService>());

            var missing = await controller.Revoke(linkId);
            var resolved = await controller.Revoke(linkId);

            Assert.IsType<NotFoundResult>(missing);
            var conflict = Assert.IsType<ConflictObjectResult>(resolved);
            var problem = Assert.IsType<ProblemDetails>(conflict.Value);
            Assert.DoesNotContain(
                linkId.ToString(),
                problem.Detail!,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Controller_UsesStaffOnlyRouteAndDedicatedPolicy()
        {
            var route = typeof(PatientIntakeAccessLinksController)
                .GetCustomAttribute<RouteAttribute>();
            var authorize = typeof(PatientIntakeAccessLinksController)
                .GetCustomAttribute<AuthorizeAttribute>();

            Assert.Equal("api/patient-intake-links", route!.Template);
            Assert.False(route.Template!.StartsWith(
                "api/patient-portal",
                StringComparison.OrdinalIgnoreCase));
            Assert.Equal(
                AuthorizationPolicies.PatientPortalIntakeManage,
                authorize!.Policy);
        }

        private static PatientIntakeAccessLinksController CreateController(
            IPatientIntakeAccessLinkCommandService commands,
            IPatientIntakeAccessLinkQueryService queries)
        {
            return new PatientIntakeAccessLinksController(commands, queries)
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
    }
}
