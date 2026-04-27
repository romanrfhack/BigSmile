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
        public async Task ConfigureManualReminder_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var appointmentId = Guid.NewGuid();
            var dueAtUtc = DateTime.UtcNow.AddHours(2);
            var response = BuildAppointmentSummary(
                "Pending",
                null,
                null,
                reminderRequired: true,
                reminderChannel: "Phone",
                reminderDueAtUtc: dueAtUtc);
            ConfigureAppointmentManualReminderCommand? capturedCommand = null;
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ConfigureManualReminderAsync(
                    appointmentId,
                    It.IsAny<ConfigureAppointmentManualReminderCommand>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, ConfigureAppointmentManualReminderCommand, CancellationToken>((_, command, _) => capturedCommand = command)
                .ReturnsAsync(response);
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ConfigureManualReminder(
                appointmentId,
                new AppointmentsController.ConfigureAppointmentManualReminderRequest
                {
                    Required = true,
                    Channel = "Phone",
                    DueAtUtc = dueAtUtc
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
            Assert.True(capturedCommand?.Required);
            Assert.Equal("Phone", capturedCommand?.Channel);
            Assert.Equal(dueAtUtc, capturedCommand?.DueAtUtc);
        }

        [Fact]
        public async Task ConfigureManualReminder_ReturnsOk_WhenClearingReminder()
        {
            var appointmentId = Guid.NewGuid();
            var response = BuildAppointmentSummary("Pending", null, null);
            ConfigureAppointmentManualReminderCommand? capturedCommand = null;
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ConfigureManualReminderAsync(
                    appointmentId,
                    It.IsAny<ConfigureAppointmentManualReminderCommand>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, ConfigureAppointmentManualReminderCommand, CancellationToken>((_, command, _) => capturedCommand = command)
                .ReturnsAsync(response);
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ConfigureManualReminder(
                appointmentId,
                new AppointmentsController.ConfigureAppointmentManualReminderRequest
                {
                    Required = false,
                    Channel = null,
                    DueAtUtc = null
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
            Assert.False(capturedCommand?.Required);
            Assert.Null(capturedCommand?.Channel);
            Assert.Null(capturedCommand?.DueAtUtc);
        }

        [Fact]
        public async Task ConfigureManualReminder_ReturnsNotFound_WhenAppointmentDoesNotExistInScope()
        {
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ConfigureManualReminderAsync(
                    appointmentId,
                    It.IsAny<ConfigureAppointmentManualReminderCommand>(),
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

            var result = await controller.ConfigureManualReminder(
                appointmentId,
                new AppointmentsController.ConfigureAppointmentManualReminderRequest
                {
                    Required = true,
                    Channel = "Phone",
                    DueAtUtc = DateTime.UtcNow.AddHours(2)
                });

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task ConfigureManualReminder_ReturnsValidationProblem_WhenChannelIsInvalid()
        {
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.ConfigureManualReminderAsync(
                    appointmentId,
                    It.IsAny<ConfigureAppointmentManualReminderCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Appointment reminder channel must be one of: Phone, WhatsApp, Email or Other."));
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ConfigureManualReminder(
                appointmentId,
                new AppointmentsController.ConfigureAppointmentManualReminderRequest
                {
                    Required = true,
                    Channel = "SMS",
                    DueAtUtc = DateTime.UtcNow.AddHours(2)
                });

            Assert.IsType<ObjectResult>(result.Result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task CompleteManualReminder_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var appointmentId = Guid.NewGuid();
            var completedAtUtc = DateTime.UtcNow;
            var response = BuildAppointmentSummary(
                "Pending",
                null,
                null,
                reminderRequired: true,
                reminderChannel: "Phone",
                reminderDueAtUtc: completedAtUtc.AddHours(-1),
                reminderCompletedAtUtc: completedAtUtc,
                reminderCompletedByUserId: Guid.NewGuid());
            var commandService = new Mock<IAppointmentCommandService>();
            commandService
                .Setup(service => service.CompleteManualReminderAsync(appointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.CompleteManualReminder(appointmentId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task ListManualReminders_ReturnsWorkItems()
        {
            var branchId = Guid.NewGuid();
            var reminders = new[]
            {
                new AppointmentReminderWorkItemDto(
                    Guid.NewGuid(),
                    branchId,
                    Guid.NewGuid(),
                    "Ana Lopez",
                    DateTime.UtcNow.AddDays(1),
                    "Scheduled",
                    "Pending",
                    "Phone",
                    DateTime.UtcNow.AddHours(2),
                    "Pending",
                    null,
                    null)
            };
            var commandService = new Mock<IAppointmentCommandService>();
            var queryService = new Mock<IAppointmentQueryService>();
            var reminderLogCommandService = new Mock<IAppointmentReminderLogCommandService>();
            var reminderLogQueryService = new Mock<IAppointmentReminderLogQueryService>();
            queryService
                .Setup(service => service.ListManualRemindersAsync(branchId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(reminders);
            var controller = new AppointmentsController(
                commandService.Object,
                queryService.Object,
                reminderLogCommandService.Object,
                reminderLogQueryService.Object);

            var result = await controller.ListManualReminders(branchId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(reminders, ok.Value);
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
            Guid? confirmedByUserId,
            bool reminderRequired = false,
            string? reminderChannel = null,
            DateTime? reminderDueAtUtc = null,
            DateTime? reminderCompletedAtUtc = null,
            Guid? reminderCompletedByUserId = null)
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
                reminderRequired,
                reminderChannel,
                reminderDueAtUtc,
                reminderCompletedAtUtc,
                reminderCompletedByUserId,
                reminderRequired ? DateTime.UtcNow : null,
                reminderRequired ? Guid.NewGuid() : null,
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
