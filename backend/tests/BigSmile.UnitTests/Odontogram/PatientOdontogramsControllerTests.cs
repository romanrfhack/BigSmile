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
        private static readonly IReadOnlyList<OdontogramSurfaceFindingDto> NoFindings = [];
        private static readonly IReadOnlyList<OdontogramSurfaceFindingHistoryEntryDto> NoFindingHistory = [];

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
                    new OdontogramToothStateDto(
                        "11",
                        "Unknown",
                        DateTime.UtcNow,
                        Guid.NewGuid(),
                        new[]
                        {
                            new OdontogramSurfaceStateDto("O", "Unknown", DateTime.UtcNow, Guid.NewGuid(), NoFindings)
                        })
                },
                NoFindingHistory,
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
                    new OdontogramToothStateDto(
                        "11",
                        "Caries",
                        DateTime.UtcNow,
                        Guid.NewGuid(),
                        new[]
                        {
                            new OdontogramSurfaceStateDto("O", "Healthy", DateTime.UtcNow, Guid.NewGuid(), NoFindings)
                        })
                },
                NoFindingHistory,
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

        [Fact]
        public async Task UpdateSurfaceStatus_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new OdontogramDetailDto(
                Guid.NewGuid(),
                patientId,
                new[]
                {
                    new OdontogramToothStateDto(
                        "11",
                        "Unknown",
                        DateTime.UtcNow,
                        Guid.NewGuid(),
                        new[]
                        {
                            new OdontogramSurfaceStateDto("O", "Caries", DateTime.UtcNow, Guid.NewGuid(), NoFindings)
                        })
                },
                NoFindingHistory,
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());

            var commandService = new Mock<IOdontogramCommandService>();
            commandService
                .Setup(service => service.UpdateSurfaceStatusAsync(
                    patientId,
                    It.IsAny<UpdateOdontogramSurfaceStatusCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IOdontogramQueryService>();
            var controller = new PatientOdontogramsController(commandService.Object, queryService.Object);

            var result = await controller.UpdateSurfaceStatus(
                patientId,
                "11",
                "O",
                new PatientOdontogramsController.UpdateSurfaceStatusRequest
                {
                    Status = "Caries"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task AddSurfaceFinding_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildOdontogramResponse(
                patientId,
                new OdontogramSurfaceFindingDto(Guid.NewGuid(), "Caries", DateTime.UtcNow, Guid.NewGuid()));

            var commandService = new Mock<IOdontogramCommandService>();
            commandService
                .Setup(service => service.AddSurfaceFindingAsync(
                    patientId,
                    It.IsAny<AddOdontogramSurfaceFindingCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IOdontogramQueryService>();
            var controller = new PatientOdontogramsController(commandService.Object, queryService.Object);

            var result = await controller.AddSurfaceFinding(
                patientId,
                "11",
                "O",
                new PatientOdontogramsController.AddSurfaceFindingRequest
                {
                    FindingType = "Caries"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task RemoveSurfaceFinding_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildOdontogramResponse(patientId);
            var findingId = Guid.NewGuid();

            var commandService = new Mock<IOdontogramCommandService>();
            commandService
                .Setup(service => service.RemoveSurfaceFindingAsync(
                    patientId,
                    "11",
                    "O",
                    findingId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IOdontogramQueryService>();
            var controller = new PatientOdontogramsController(commandService.Object, queryService.Object);

            var result = await controller.RemoveSurfaceFinding(patientId, "11", "O", findingId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        private static OdontogramDetailDto BuildOdontogramResponse(
            Guid patientId,
            params OdontogramSurfaceFindingDto[] findings)
        {
            return new OdontogramDetailDto(
                Guid.NewGuid(),
                patientId,
                new[]
                {
                    new OdontogramToothStateDto(
                        "11",
                        "Unknown",
                        DateTime.UtcNow,
                        Guid.NewGuid(),
                        new[]
                        {
                            new OdontogramSurfaceStateDto(
                                "O",
                                "Unknown",
                                DateTime.UtcNow,
                                Guid.NewGuid(),
                                findings)
                        })
                },
                NoFindingHistory,
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());
        }
    }
}
