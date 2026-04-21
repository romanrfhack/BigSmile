using BigSmile.Api.Controllers;
using BigSmile.Application.Features.TreatmentPlans.Commands;
using BigSmile.Application.Features.TreatmentPlans.Dtos;
using BigSmile.Application.Features.TreatmentPlans.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.TreatmentPlans
{
    public class PatientTreatmentPlansControllerTests
    {
        [Fact]
        public async Task GetByPatientId_ReturnsNotFound_WhenTreatmentPlanDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<ITreatmentPlanCommandService>();
            var queryService = new Mock<ITreatmentPlanQueryService>();
            queryService
                .Setup(service => service.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TreatmentPlanDetailDto?)null);

            var controller = new PatientTreatmentPlansController(commandService.Object, queryService.Object);

            var result = await controller.GetByPatientId(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildTreatmentPlanResponse(patientId);
            var commandService = new Mock<ITreatmentPlanCommandService>();
            commandService
                .Setup(service => service.CreateAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentPlanQueryService>();
            var controller = new PatientTreatmentPlansController(commandService.Object, queryService.Object);

            var result = await controller.Create(patientId);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientTreatmentPlansController.GetByPatientId), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task AddItem_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildTreatmentPlanResponse(
                patientId,
                new TreatmentPlanItemDto(
                    Guid.NewGuid(),
                    "Composite restoration",
                    "Restorative",
                    1,
                    null,
                    "11",
                    "O",
                    DateTime.UtcNow,
                    Guid.NewGuid()));

            var commandService = new Mock<ITreatmentPlanCommandService>();
            commandService
                .Setup(service => service.AddItemAsync(
                    patientId,
                    It.IsAny<AddTreatmentPlanItemCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentPlanQueryService>();
            var controller = new PatientTreatmentPlansController(commandService.Object, queryService.Object);

            var result = await controller.AddItem(
                patientId,
                new PatientTreatmentPlansController.AddTreatmentPlanItemRequest
                {
                    Title = "Composite restoration",
                    Category = "Restorative",
                    Quantity = 1,
                    ToothCode = "11",
                    SurfaceCode = "O"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task RemoveItem_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var response = BuildTreatmentPlanResponse(patientId);
            var commandService = new Mock<ITreatmentPlanCommandService>();
            commandService
                .Setup(service => service.RemoveItemAsync(
                    patientId,
                    itemId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentPlanQueryService>();
            var controller = new PatientTreatmentPlansController(commandService.Object, queryService.Object);

            var result = await controller.RemoveItem(patientId, itemId);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildTreatmentPlanResponse(patientId) with { Status = "Proposed" };
            var commandService = new Mock<ITreatmentPlanCommandService>();
            commandService
                .Setup(service => service.ChangeStatusAsync(
                    patientId,
                    It.IsAny<ChangeTreatmentPlanStatusCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentPlanQueryService>();
            var controller = new PatientTreatmentPlansController(commandService.Object, queryService.Object);

            var result = await controller.ChangeStatus(
                patientId,
                new PatientTreatmentPlansController.ChangeTreatmentPlanStatusRequest
                {
                    Status = "Proposed"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        private static TreatmentPlanDetailDto BuildTreatmentPlanResponse(
            Guid patientId,
            params TreatmentPlanItemDto[] items)
        {
            return new TreatmentPlanDetailDto(
                Guid.NewGuid(),
                patientId,
                "Draft",
                items,
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());
        }
    }
}
