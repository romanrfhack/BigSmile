using BigSmile.Api.Controllers;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Features.Scheduling.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.Scheduling
{
    public class AppointmentsControllerTests
    {
        [Fact]
        public async Task ChangeConfirmation_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var appointmentId = Guid.NewGuid();
            var response = BuildAppointmentSummary("Confirmed", DateTime.UtcNow, Guid.NewGuid());
            ChangeAppointmentConfirmationCommand? capturedCommand = null;
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ChangeConfirmationAsync(
                    appointmentId,
                    It.IsAny<ChangeAppointmentConfirmationCommand>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, ChangeAppointmentConfirmationCommand, CancellationToken>((_, command, _) => capturedCommand = command)
                .ReturnsAsync(response);
            var queryService = new Mock<IAppointmentQueryService>();
            var controller = new AppointmentsController(commandService.Object, queryService.Object);

            var result = await controller.ChangeConfirmation(
                appointmentId,
                new AppointmentsController.ChangeAppointmentConfirmationRequest
                {
                    Status = "Confirmed"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
            Assert.Equal("Confirmed", capturedCommand?.Status);
        }

        [Fact]
        public async Task ChangeConfirmation_ReturnsNotFound_WhenAppointmentDoesNotExistInScope()
        {
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ChangeConfirmationAsync(
                    appointmentId,
                    It.IsAny<ChangeAppointmentConfirmationCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentSummaryDto?)null);
            var queryService = new Mock<IAppointmentQueryService>();
            var controller = new AppointmentsController(commandService.Object, queryService.Object);

            var result = await controller.ChangeConfirmation(
                appointmentId,
                new AppointmentsController.ChangeAppointmentConfirmationRequest
                {
                    Status = "Confirmed"
                });

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task ChangeConfirmation_ReturnsValidationProblem_WhenStatusIsInvalid()
        {
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ChangeConfirmationAsync(
                    appointmentId,
                    It.IsAny<ChangeAppointmentConfirmationCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Appointment confirmation status must be one of: Pending or Confirmed."));
            var queryService = new Mock<IAppointmentQueryService>();
            var controller = new AppointmentsController(commandService.Object, queryService.Object);

            var result = await controller.ChangeConfirmation(
                appointmentId,
                new AppointmentsController.ChangeAppointmentConfirmationRequest
                {
                    Status = "Contacted"
                });

            Assert.IsType<ObjectResult>(result.Result);
            Assert.False(controller.ModelState.IsValid);
        }

        private static AppointmentSummaryDto BuildAppointmentSummary(
            string confirmationStatus,
            DateTime? confirmedAtUtc,
            Guid? confirmedByUserId)
        {
            return new AppointmentSummaryDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Ana Lopez",
                new DateTime(2026, 4, 25, 9, 0, 0),
                new DateTime(2026, 4, 25, 9, 30, 0),
                "Scheduled",
                confirmationStatus,
                confirmedAtUtc,
                confirmedByUserId,
                "Follow-up",
                null);
        }
    }
}
