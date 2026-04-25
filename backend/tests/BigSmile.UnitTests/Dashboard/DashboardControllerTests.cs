using BigSmile.Api.Controllers;
using BigSmile.Application.Features.Dashboard.Dtos;
using BigSmile.Application.Features.Dashboard.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.Dashboard
{
    public sealed class DashboardControllerTests
    {
        [Fact]
        public async Task GetSummary_ReturnsOk_WhenQueryServiceSucceeds()
        {
            var summary = new DashboardSummaryDto(
                ActivePatientsCount: 2,
                TodayAppointmentsCount: 3,
                TodayPendingAppointmentsCount: 1,
                ActiveDocumentsCount: 4,
                ActiveTreatmentPlansCount: 5,
                AcceptedQuotesCount: 6,
                IssuedBillingDocumentsCount: 7,
                GeneratedAtUtc: DateTime.UtcNow);
            var queryService = new Mock<IDashboardSummaryQueryService>();
            queryService
                .Setup(service => service.GetSummaryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(summary);
            var controller = new DashboardController(queryService.Object);

            var result = await controller.GetSummary();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(summary, ok.Value);
        }

        [Fact]
        public async Task GetSummary_ReturnsValidationProblem_WhenTenantContextIsMissing()
        {
            var queryService = new Mock<IDashboardSummaryQueryService>();
            queryService
                .Setup(service => service.GetSummaryAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Dashboard summary requires a resolved tenant context."));
            var controller = new DashboardController(queryService.Object);

            var result = await controller.GetSummary();

            Assert.IsType<ObjectResult>(result.Result);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}
