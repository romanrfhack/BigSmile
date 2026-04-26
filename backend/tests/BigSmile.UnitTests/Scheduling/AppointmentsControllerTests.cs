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
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

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
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

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
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ChangeConfirmation(
                appointmentId,
                new AppointmentsController.ChangeAppointmentConfirmationRequest
                {
                    Status = "Contacted"
                });

            Assert.IsType<ObjectResult>(result.Result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task ListReminderLog_ReturnsEntries_WhenAppointmentExistsInScope()
        {
            var appointmentId = Guid.NewGuid();
            var entries = new[]
            {
                BuildReminderLogEntry(appointmentId, "Phone", "Reached", DateTime.UtcNow)
            };
            var commandService = new Mock<IAppointmentCommandService>();
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            reminderLogQueryService
                .Setup(service => service.ListAsync(appointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entries);
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ListReminderLog(appointmentId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(entries, ok.Value);
        }

        [Fact]
        public async Task ListReminderLog_ReturnsNotFound_WhenAppointmentDoesNotExistInScope()
        {
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IAppointmentCommandService>();
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            reminderLogQueryService
                .Setup(service => service.ListAsync(appointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<AppointmentReminderLogEntryDto>?)null);
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ListReminderLog(appointmentId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddReminderLogEntry_ReturnsCreated_WhenCommandServiceSucceeds()
        {
            var appointmentId = Guid.NewGuid();
            var response = BuildReminderLogEntry(appointmentId, "WhatsApp", "NoAnswer", DateTime.UtcNow);
            AddAppointmentReminderLogEntryCommand? capturedCommand = null;
            var commandService = new Mock<IAppointmentCommandService>();
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            reminderLogCommandService
                .Setup(service => service.AddAsync(
                    appointmentId,
                    It.IsAny<AddAppointmentReminderLogEntryCommand>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, AddAppointmentReminderLogEntryCommand, CancellationToken>((_, command, _) => capturedCommand = command)
                .ReturnsAsync(response);
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.AddReminderLogEntry(
                appointmentId,
                new AppointmentsController.AddAppointmentReminderLogEntryRequest
                {
                    Channel = "WhatsApp",
                    Outcome = "NoAnswer",
                    Notes = "Tried outside the system."
                });

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(AppointmentsController.ListReminderLog), created.ActionName);
            Assert.Same(response, created.Value);
            Assert.Equal("WhatsApp", capturedCommand?.Channel);
            Assert.Equal("NoAnswer", capturedCommand?.Outcome);
            Assert.Equal("Tried outside the system.", capturedCommand?.Notes);
        }

        [Fact]
        public async Task AddReminderLogEntry_ReturnsValidationProblem_WhenChannelIsInvalid()
        {
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IAppointmentCommandService>();
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            reminderLogCommandService
                .Setup(service => service.AddAsync(
                    appointmentId,
                    It.IsAny<AddAppointmentReminderLogEntryCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Appointment reminder channel must be one of: Phone, WhatsApp, Email or Other."));
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.AddReminderLogEntry(
                appointmentId,
                new AppointmentsController.AddAppointmentReminderLogEntryRequest
                {
                    Channel = "SMS",
                    Outcome = "Reached"
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

        private static AppointmentReminderLogEntryDto BuildReminderLogEntry(
            Guid appointmentId,
            string channel,
            string outcome,
            DateTime createdAtUtc)
        {
            return new AppointmentReminderLogEntryDto(
                Guid.NewGuid(),
                appointmentId,
                channel,
                outcome,
                "Manual contact attempt.",
                createdAtUtc,
                Guid.NewGuid());
        }
    }
}
