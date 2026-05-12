using BigSmile.Api.Controllers;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.ClinicalRecords.Commands;
using BigSmile.Application.Features.ClinicalRecords.Dtos;
using BigSmile.Application.Features.ClinicalRecords.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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

        [Fact]
        public async Task GetQuestionnaire_ReturnsNotFound_WhenClinicalRecordDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IClinicalRecordCommandService>();
            var queryService = new Mock<IClinicalRecordQueryService>();
            queryService
                .Setup(service => service.GetQuestionnaireByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicalMedicalQuestionnaireDto?)null);

            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.GetQuestionnaire(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateQuestionnaire_ReturnsOk_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new ClinicalMedicalQuestionnaireDto(
                Guid.NewGuid(),
                patientId,
                new[]
                {
                    new ClinicalMedicalAnswerDto(Guid.NewGuid(), "diabetes", "No", null, DateTime.UtcNow, Guid.NewGuid())
                });

            var commandService = new Mock<IClinicalRecordCommandService>();
            commandService
                .Setup(service => service.UpdateQuestionnaireAsync(
                    patientId,
                    It.IsAny<SaveClinicalMedicalQuestionnaireCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IClinicalRecordQueryService>();
            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.UpdateQuestionnaire(
                patientId,
                new PatientClinicalRecordsController.SaveClinicalMedicalQuestionnaireRequest
                {
                    Answers = new List<PatientClinicalRecordsController.ClinicalMedicalAnswerRequest>
                    {
                        new()
                        {
                            QuestionKey = "diabetes",
                            Answer = "No"
                        }
                    }
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
        }

        [Fact]
        public void QuestionnaireEndpoints_RequireClinicalReadAndClinicalWritePolicies()
        {
            var getAuthorize = typeof(PatientClinicalRecordsController)
                .GetMethod(nameof(PatientClinicalRecordsController.GetQuestionnaire))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
                .Cast<AuthorizeAttribute>()
                .Single();
            var putAuthorize = typeof(PatientClinicalRecordsController)
                .GetMethod(nameof(PatientClinicalRecordsController.UpdateQuestionnaire))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
                .Cast<AuthorizeAttribute>()
                .Single();

            Assert.Equal(AuthorizationPolicies.ClinicalRead, getAuthorize.Policy);
            Assert.Equal(AuthorizationPolicies.ClinicalWrite, putAuthorize.Policy);
        }

        [Fact]
        public void SaveClinicalMedicalQuestionnaireRequest_RejectsInvalidQuestionKey()
        {
            var request = new PatientClinicalRecordsController.SaveClinicalMedicalQuestionnaireRequest
            {
                Answers = new List<PatientClinicalRecordsController.ClinicalMedicalAnswerRequest>
                {
                    new()
                    {
                        QuestionKey = "unknownQuestion",
                        Answer = "Yes"
                    }
                }
            };

            var results = Validate(request);

            Assert.Contains(results, result => result.MemberNames.Any(member => member.Contains(nameof(PatientClinicalRecordsController.ClinicalMedicalAnswerRequest.QuestionKey))));
        }

        [Fact]
        public void SaveClinicalMedicalQuestionnaireRequest_RejectsInvalidAnswer()
        {
            var request = new PatientClinicalRecordsController.SaveClinicalMedicalQuestionnaireRequest
            {
                Answers = new List<PatientClinicalRecordsController.ClinicalMedicalAnswerRequest>
                {
                    new()
                    {
                        QuestionKey = "diabetes",
                        Answer = "Maybe"
                    }
                }
            };

            var results = Validate(request);

            Assert.Contains(results, result => result.MemberNames.Any(member => member.Contains(nameof(PatientClinicalRecordsController.ClinicalMedicalAnswerRequest.Answer))));
        }

        [Fact]
        public void SaveClinicalMedicalQuestionnaireRequest_RejectsDuplicateQuestionKeys()
        {
            var request = new PatientClinicalRecordsController.SaveClinicalMedicalQuestionnaireRequest
            {
                Answers = new List<PatientClinicalRecordsController.ClinicalMedicalAnswerRequest>
                {
                    new()
                    {
                        QuestionKey = "diabetes",
                        Answer = "Yes"
                    },
                    new()
                    {
                        QuestionKey = " diabetes ",
                        Answer = "No"
                    }
                }
            };

            var results = Validate(request);

            Assert.Contains(results, result => result.ErrorMessage!.Contains("duplicate question keys", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void SaveClinicalMedicalQuestionnaireRequest_RejectsNumericAnswerValues()
        {
            var request = new PatientClinicalRecordsController.SaveClinicalMedicalQuestionnaireRequest
            {
                Answers = new List<PatientClinicalRecordsController.ClinicalMedicalAnswerRequest>
                {
                    new()
                    {
                        QuestionKey = "diabetes",
                        Answer = "1"
                    }
                }
            };

            var results = Validate(request);

            Assert.Contains(results, result => result.MemberNames.Any(member => member.Contains(nameof(PatientClinicalRecordsController.ClinicalMedicalAnswerRequest.Answer))));
        }

        [Fact]
        public void SaveClinicalMedicalQuestionnaireRequest_DoesNotAcceptTenantIdFromJson()
        {
            var json = """
                {
                  "tenantId": "4bff6ec8-7a0f-4b5c-bcd7-2c60778b3526",
                  "answers": [
                    {
                      "questionKey": "diabetes",
                      "answer": "No"
                    }
                  ]
                }
                """;
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<PatientClinicalRecordsController.SaveClinicalMedicalQuestionnaireRequest>(json, options));
        }

        [Fact]
        public void ClinicalMedicalAnswerRequest_DoesNotAcceptTenantIdFromJson()
        {
            var json = """
                {
                  "questionKey": "diabetes",
                  "answer": "No",
                  "tenantId": "4bff6ec8-7a0f-4b5c-bcd7-2c60778b3526"
                }
                """;
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<PatientClinicalRecordsController.ClinicalMedicalAnswerRequest>(json, options));
        }

        private static IReadOnlyCollection<ValidationResult> Validate(object value)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);

            return results;
        }
    }
}
