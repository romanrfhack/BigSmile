using BigSmile.Api.Controllers;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.Clinical
{
    public class PatientClinicalRecordsControllerTests
    {
        [Fact]
        public async Task GetByPatientId_ReturnsNotFound_WhenClinicalRecordDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IClinicalRecordCommandService>();
            var queryService = new Mock<IClinicalRecordQueryService>();
            queryService
                .Setup(service => service.GetByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicalRecordDetailDto?)null);

            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.GetByPatientId(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddNote_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new ClinicalRecordDetailDto(
                Guid.NewGuid(),
                patientId,
                "Background",
                null,
                Array.Empty<ClinicalAllergyEntryDto>(),
                new[]
                {
                    new ClinicalNoteDto(Guid.NewGuid(), "Newest note", DateTime.UtcNow, Guid.NewGuid())
                },
                Array.Empty<ClinicalDiagnosisDto>(),
                Array.Empty<ClinicalSnapshotHistoryEntryDto>(),
                Array.Empty<ClinicalTimelineEntryDto>(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());

            var commandService = new Mock<IClinicalRecordCommandService>();
            commandService
                .Setup(service => service.AddNoteAsync(
                    patientId,
                    It.IsAny<AddClinicalNoteCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IClinicalRecordQueryService>();
            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.AddNote(
                patientId,
                new PatientClinicalRecordsController.AddClinicalNoteRequest
                {
                    NoteText = "Newest note"
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public async Task AddDiagnosis_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new ClinicalRecordDetailDto(
                Guid.NewGuid(),
                patientId,
                "Background",
                null,
                Array.Empty<ClinicalAllergyEntryDto>(),
                Array.Empty<ClinicalNoteDto>(),
                new[]
                {
                    new ClinicalDiagnosisDto(
                        Guid.NewGuid(),
                        "Occlusal caries",
                        "Upper molar.",
                        "Active",
                        DateTime.UtcNow,
                        Guid.NewGuid(),
                        null,
                        null)
                },
                Array.Empty<ClinicalSnapshotHistoryEntryDto>(),
                Array.Empty<ClinicalTimelineEntryDto>(),
                DateTime.UtcNow,
                Guid.NewGuid(),
                DateTime.UtcNow,
                Guid.NewGuid());

            var commandService = new Mock<IClinicalRecordCommandService>();
            commandService
                .Setup(service => service.AddDiagnosisAsync(
                    patientId,
                    It.IsAny<AddClinicalDiagnosisCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IClinicalRecordQueryService>();
            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.AddDiagnosis(
                patientId,
                new PatientClinicalRecordsController.AddClinicalDiagnosisRequest
                {
                    DiagnosisText = "Occlusal caries",
                    Notes = "Upper molar."
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }
    }
}
