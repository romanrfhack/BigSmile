using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientPortalInvitations.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using BigSmile.Application.Features.PatientPortalInvitations.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalInvitationsControllerTests
    {
        [Fact]
        public async Task List_ReturnsNotFound_WhenPatientIsUnavailable()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IPatientPortalInvitationCommandService>();
            var queryService = new Mock<IPatientPortalInvitationQueryService>();
            queryService
                .Setup(service => service.ListAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PatientPortalInvitationSummaryDto>?)null);
            var controller = CreateController(commandService.Object, queryService.Object);

            var result = await controller.List(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Issue_ReturnsCreatedAndPassesRequestCorrelationId()
        {
            var patientId = Guid.NewGuid();
            var response = new IssuedPatientPortalInvitationDto(
                Guid.NewGuid(),
                patientId,
                "ExistingPatientActivation",
                "raw-one-time-token",
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(24));
            var commandService = new Mock<IPatientPortalInvitationCommandService>();
            commandService
                .Setup(service => service.IssueAsync(
                    patientId,
                    "trace-123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var queryService = new Mock<IPatientPortalInvitationQueryService>();
            var controller = CreateController(commandService.Object, queryService.Object, "trace-123");

            var result = await controller.Issue(patientId);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientPortalInvitationsController.List), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task Revoke_ReturnsNoContent_WhenInvitationIsRevoked()
        {
            var patientId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();
            var commandService = new Mock<IPatientPortalInvitationCommandService>();
            commandService
                .Setup(service => service.RevokeAsync(
                    patientId,
                    invitationId,
                    "trace-123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var queryService = new Mock<IPatientPortalInvitationQueryService>();
            var controller = CreateController(commandService.Object, queryService.Object, "trace-123");

            var result = await controller.Revoke(patientId, invitationId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Revoke_ReturnsConflict_WhenLifecycleRejectsTheChange()
        {
            var patientId = Guid.NewGuid();
            var invitationId = Guid.NewGuid();
            var commandService = new Mock<IPatientPortalInvitationCommandService>();
            commandService
                .Setup(service => service.RevokeAsync(
                    patientId,
                    invitationId,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Invitation is already consumed."));
            var queryService = new Mock<IPatientPortalInvitationQueryService>();
            var controller = CreateController(commandService.Object, queryService.Object);

            var result = await controller.Revoke(patientId, invitationId);

            var conflict = Assert.IsType<ConflictObjectResult>(result);
            var problem = Assert.IsType<ProblemDetails>(conflict.Value);
            Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        }

        private static PatientPortalInvitationsController CreateController(
            IPatientPortalInvitationCommandService commandService,
            IPatientPortalInvitationQueryService queryService,
            string traceIdentifier = "test-trace")
        {
            return new PatientPortalInvitationsController(commandService, queryService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        TraceIdentifier = traceIdentifier
                    }
                }
            };
        }
    }
}
