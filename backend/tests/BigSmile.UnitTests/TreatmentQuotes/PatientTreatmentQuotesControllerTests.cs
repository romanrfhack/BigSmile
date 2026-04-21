using BigSmile.Api.Controllers;
using BigSmile.Application.Features.TreatmentQuotes.Commands;
using BigSmile.Application.Features.TreatmentQuotes.Dtos;
using BigSmile.Application.Features.TreatmentQuotes.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.TreatmentQuotes
{
    public class PatientTreatmentQuotesControllerTests
    {
        [Fact]
        public async Task GetByPatientId_ReturnsNotFound_WhenTreatmentQuoteDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<ITreatmentQuoteCommandService>();
            var queryService = new Mock<ITreatmentQuoteQueryService>();
            queryService
                .Setup(service => service.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TreatmentQuoteDetailDto?)null);

            var controller = new PatientTreatmentQuotesController(commandService.Object, queryService.Object);

            var result = await controller.GetByPatientId(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildTreatmentQuoteResponse(patientId);
            var commandService = new Mock<ITreatmentQuoteCommandService>();
            commandService
                .Setup(service => service.CreateAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentQuoteQueryService>();
            var controller = new PatientTreatmentQuotesController(commandService.Object, queryService.Object);

            var result = await controller.Create(patientId);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientTreatmentQuotesController.GetByPatientId), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task UpdateItemUnitPrice_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var quoteItemId = Guid.NewGuid();
            var response = BuildTreatmentQuoteResponse(
                patientId,
                new TreatmentQuoteItemDto(
                    quoteItemId,
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
                    Guid.NewGuid()));

            var commandService = new Mock<ITreatmentQuoteCommandService>();
            commandService
                .Setup(service => service.UpdateItemUnitPriceAsync(
                    patientId,
                    quoteItemId,
                    It.IsAny<UpdateTreatmentQuoteItemPriceCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentQuoteQueryService>();
            var controller = new PatientTreatmentQuotesController(commandService.Object, queryService.Object);

            var result = await controller.UpdateItemUnitPrice(
                patientId,
                quoteItemId,
                new PatientTreatmentQuotesController.UpdateTreatmentQuoteItemPriceRequest
                {
                    UnitPrice = 450m
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task ChangeStatus_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildTreatmentQuoteResponse(patientId) with { Status = "Proposed" };
            var commandService = new Mock<ITreatmentQuoteCommandService>();
            commandService
                .Setup(service => service.ChangeStatusAsync(
                    patientId,
                    It.IsAny<ChangeTreatmentQuoteStatusCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<ITreatmentQuoteQueryService>();
            var controller = new PatientTreatmentQuotesController(commandService.Object, queryService.Object);

            var result = await controller.ChangeStatus(
                patientId,
                new PatientTreatmentQuotesController.ChangeTreatmentQuoteStatusRequest
                {
                    Status = "Proposed"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        private static TreatmentQuoteDetailDto BuildTreatmentQuoteResponse(
            Guid patientId,
            params TreatmentQuoteItemDto[] items)
        {
            return new TreatmentQuoteDetailDto(
                Guid.NewGuid(),
                patientId,
                Guid.NewGuid(),
                "Draft",
                "MXN",
                items.Sum(item => item.LineTotal),
                items,
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());
        }
    }
}
