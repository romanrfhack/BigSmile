using BigSmile.Api.Controllers;
using BigSmile.Application.Features.Odontograms.Commands;
using BigSmile.Application.Features.Odontograms.Dtos;
using BigSmile.Application.Features.Odontograms.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.Odontogram
{
    public class PatientOdontogramsControllerTests
    {
        [Fact]
        public async Task GetByPatientId_ReturnsNotFound_WhenOdontogramDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IOdontogramCommandService>();
            var queryService = new Mock<IOdontogramQueryService>();
            queryService
                .Setup(service => service.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((OdontogramDetailDto?)null);

            var controller = new PatientOdontogramsController(commandService.Object, queryService.Object);

            var result = await controller.GetByPatientId(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new OdontogramDetailDto(
                Guid.NewGuid(),
                patientId,
                new[]
                {
                    new OdontogramToothStateDto("11", "Unknown", DateTime.UtcNow, Guid.NewGuid())
                },
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());

            var commandService = new Mock<IOdontogramCommandService>();
            commandService
                .Setup(service => service.CreateAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IOdontogramQueryService>();
            var controller = new PatientOdontogramsController(commandService.Object, queryService.Object);

            var result = await controller.Create(patientId);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientOdontogramsController.GetByPatientId), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task UpdateToothStatus_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new OdontogramDetailDto(
                Guid.NewGuid(),
                patientId,
                new[]
                {
                    new OdontogramToothStateDto("11", "Caries", DateTime.UtcNow, Guid.NewGuid())
                },
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());

            var commandService = new Mock<IOdontogramCommandService>();
            commandService
                .Setup(service => service.UpdateToothStatusAsync(
                    patientId,
                    It.IsAny<UpdateOdontogramToothStatusCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IOdontogramQueryService>();
            var controller = new PatientOdontogramsController(commandService.Object, queryService.Object);

            var result = await controller.UpdateToothStatus(
                patientId,
                "11",
                new PatientOdontogramsController.UpdateToothStatusRequest
                {
                    Status = "Caries"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }
    }
}
