using System.Text;
using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientDocuments.Commands;
using BigSmile.Application.Features.PatientDocuments.Dtos;
using BigSmile.Application.Features.PatientDocuments.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.Documents
{
    public class PatientDocumentsControllerTests
    {
        [Fact]
        public async Task ListActive_ReturnsNotFound_WhenPatientDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IPatientDocumentCommandService>();
            var queryService = new Mock<IPatientDocumentQueryService>();
            queryService
                .Setup(service => service.ListActiveByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PatientDocumentSummaryDto>?)null);

            var controller = new PatientDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.ListActive(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Upload_ReturnsCreatedAtAction_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = BuildDocumentResponse(patientId);
            var commandService = new Mock<IPatientDocumentCommandService>();
            commandService
                .Setup(service => service.UploadAsync(
                    patientId,
                    It.IsAny<UploadPatientDocumentCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IPatientDocumentQueryService>();
            var controller = new PatientDocumentsController(commandService.Object, queryService.Object);

            var request = new PatientDocumentsController.UploadPatientDocumentRequest
            {
                File = BuildFormFile("radiography.pdf", "application/pdf", "fake pdf")
            };

            var result = await controller.Upload(patientId, request);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientDocumentsController.ListActive), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public async Task Download_ReturnsNotFound_WhenDocumentDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var commandService = new Mock<IPatientDocumentCommandService>();
            var queryService = new Mock<IPatientDocumentQueryService>();
            queryService
                .Setup(service => service.DownloadAsync(patientId, documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PatientDocumentDownloadResult?)null);

            var controller = new PatientDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.Download(patientId, documentId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Download_ReturnsFileResult_WhenDocumentExists()
        {
            var patientId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var contentStream = new MemoryStream(Encoding.UTF8.GetBytes("pdf-content"));
            var commandService = new Mock<IPatientDocumentCommandService>();
            var queryService = new Mock<IPatientDocumentQueryService>();
            queryService
                .Setup(service => service.DownloadAsync(patientId, documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PatientDocumentDownloadResult(
                    "radiography.pdf",
                    "application/pdf",
                    contentStream));

            var controller = new PatientDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.Download(patientId, documentId);

            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal("radiography.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task Retire_ReturnsNoContent_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var documentId = Guid.NewGuid();
            var commandService = new Mock<IPatientDocumentCommandService>();
            commandService
                .Setup(service => service.RetireAsync(patientId, documentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var queryService = new Mock<IPatientDocumentQueryService>();
            var controller = new PatientDocumentsController(commandService.Object, queryService.Object);

            var result = await controller.Retire(patientId, documentId);

            Assert.IsType<NoContentResult>(result);
        }

        private static PatientDocumentSummaryDto BuildDocumentResponse(Guid patientId)
        {
            return new PatientDocumentSummaryDto(
                Guid.NewGuid(),
                patientId,
                "radiography.pdf",
                "application/pdf",
                2048,
                DateTime.UtcNow,
                Guid.NewGuid());
        }

        private static IFormFile BuildFormFile(string fileName, string contentType, string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }
    }
}
