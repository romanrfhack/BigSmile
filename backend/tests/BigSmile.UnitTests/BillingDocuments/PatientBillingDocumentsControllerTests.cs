using BigSmile.Api.Controllers;
using BigSmile.Application.Features.BillingDocuments.Commands;
using BigSmile.Application.Features.BillingDocuments.Dtos;
using BigSmile.Application.Features.BillingDocuments.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.BillingDocuments
{
    public class PatientBillingDocumentsControllerTests
    {
        [Fact]
        public async Task GetByPatientId_ReturnsNotFound_WhenBillingDocumentDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IBillingDocumentCommandService>();
            var queryService = new Mock<IBillingDocumentQueryService>();
            queryService
                .Setup(service => service.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BillingDocumentDetailDto?)null);

            var controller = new PatientBillingDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.GetByPatientId(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildBillingDocumentResponse(patientId);
            var commandService = new Mock<IBillingDocumentCommandService>();
            commandService
                .Setup(service => service.CreateAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IBillingDocumentQueryService>();
            var controller = new PatientBillingDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.Create(patientId);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientBillingDocumentsController.GetByPatientId), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildBillingDocumentResponse(patientId) with { Status = "Issued" };
            var commandService = new Mock<IBillingDocumentCommandService>();
            commandService
                .Setup(service => service.ChangeStatusAsync(
                    patientId,
                    It.IsAny<ChangeBillingDocumentStatusCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IBillingDocumentQueryService>();
            var controller = new PatientBillingDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.ChangeStatus(
                patientId,
                new PatientBillingDocumentsController.ChangeBillingDocumentStatusRequest
                {
                    Status = "Issued"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        private static BillingDocumentDetailDto BuildBillingDocumentResponse(Guid patientId)
        {
            return new BillingDocumentDetailDto(
                Guid.NewGuid(),
                patientId,
                Guid.NewGuid(),
                "Draft",
                "MXN",
                450m,
                new[]
                {
                    new BillingDocumentItemDto(
                        Guid.NewGuid(),
                        Guid.NewGuid(),
                        "Composite restoration",
                        "Restorative",
                        1,
                        null,
                        "11",
                        "O",
                        450m,
                        450m,
                        DateTime.UtcNow,
                        Guid.NewGuid())
                },
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                null,
                null);
        }
    }
}
