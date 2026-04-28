using BigSmile.Api.Controllers;
using BigSmile.Application.Features.Scheduling.Commands;
using BigSmile.Application.Features.Scheduling.Dtos;
using BigSmile.Application.Features.Scheduling.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.Scheduling
{
    public sealed class ReminderTemplatesControllerTests
    {
        [Fact]
        public async Task List_ReturnsTemplatesFromQueryService()
        {
            var templates = new[] { BuildTemplate("Confirmacion", isActive: true) };
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            queryService
                .Setup(service => service.ListAsync(false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(templates);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.List();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(templates, ok.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedTemplateAndDoesNotAcceptTenantId()
        {
            var response = BuildTemplate("Confirmacion", isActive: true);
            CreateReminderTemplateCommand? capturedCommand = null;
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            commandService
                .Setup(service => service.CreateAsync(
                    It.IsAny<CreateReminderTemplateCommand>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CreateReminderTemplateCommand, CancellationToken>((command, _) => capturedCommand = command)
                .ReturnsAsync(response);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Create(new ReminderTemplatesController.SaveReminderTemplateRequest
            {
                Name = "Confirmacion",
                Body = "Hola {{patientName}}."
            });

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ReminderTemplatesController.List), created.ActionName);
            Assert.Same(response, created.Value);
            Assert.Equal("Confirmacion", capturedCommand?.Name);
            Assert.Equal("Hola {{patientName}}.", capturedCommand?.Body);
        }

        [Fact]
        public async Task Create_ReturnsValidationProblemForInvalidNameOrBody()
        {
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            commandService
                .Setup(service => service.CreateAsync(
                    It.IsAny<CreateReminderTemplateCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("Reminder template name is required."));
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Create(new ReminderTemplatesController.SaveReminderTemplateRequest
            {
                Name = " ",
                Body = "Hola."
            });

            Assert.IsType<ObjectResult>(result.Result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Update_ReturnsOkForActiveTemplate()
        {
            var templateId = Guid.NewGuid();
            var response = BuildTemplate("Updated", isActive: true);
            UpdateReminderTemplateCommand? capturedCommand = null;
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            commandService
                .Setup(service => service.UpdateAsync(
                    templateId,
                    It.IsAny<UpdateReminderTemplateCommand>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateReminderTemplateCommand, CancellationToken>((_, command, _) => capturedCommand = command)
                .ReturnsAsync(response);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Update(templateId, new ReminderTemplatesController.SaveReminderTemplateRequest
            {
                Name = "Updated",
                Body = "Updated body"
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
            Assert.Equal("Updated", capturedCommand?.Name);
            Assert.Equal("Updated body", capturedCommand?.Body);
        }

        [Fact]
        public async Task Update_ReturnsNotFoundForTemplateOutsideTenant()
        {
            var templateId = Guid.NewGuid();
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            commandService
                .Setup(service => service.UpdateAsync(
                    templateId,
                    It.IsAny<UpdateReminderTemplateCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReminderTemplateDto?)null);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Update(templateId, new ReminderTemplatesController.SaveReminderTemplateRequest
            {
                Name = "Updated",
                Body = "Updated body"
            });

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Deactivate_ReturnsNoContentWhenTemplateExists()
        {
            var templateId = Guid.NewGuid();
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            commandService
                .Setup(service => service.DeactivateAsync(templateId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Deactivate(templateId);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Preview_ReturnsRenderedTemplate()
        {
            var templateId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();
            var preview = new ReminderTemplatePreviewDto(
                templateId,
                appointmentId,
                "Hola Ana.",
                Array.Empty<string>());
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            queryService
                .Setup(service => service.PreviewAsync(templateId, appointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(preview);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Preview(templateId, new ReminderTemplatesController.PreviewReminderTemplateRequest
            {
                AppointmentId = appointmentId
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(preview, ok.Value);
        }

        [Fact]
        public async Task Preview_ReturnsNotFoundForTemplateOrAppointmentOutsideScope()
        {
            var templateId = Guid.NewGuid();
            var appointmentId = Guid.NewGuid();
            var commandService = new Mock<IReminderTemplateCommandService>();
            var queryService = new Mock<IReminderTemplateQueryService>();
            queryService
                .Setup(service => service.PreviewAsync(templateId, appointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReminderTemplatePreviewDto?)null);
            var controller = new ReminderTemplatesController(commandService.Object, queryService.Object);

            var result = await controller.Preview(templateId, new ReminderTemplatesController.PreviewReminderTemplateRequest
            {
                AppointmentId = appointmentId
            });

            Assert.IsType<NotFoundResult>(result.Result);
        }

        private static ReminderTemplateDto BuildTemplate(string name, bool isActive)
        {
            return new ReminderTemplateDto(
                Guid.NewGuid(),
                name,
                "Hola {{patientName}}.",
                isActive,
                DateTime.UtcNow,
                Guid.NewGuid(),
                null,
                null,
                null,
                null);
        }
    }
}
