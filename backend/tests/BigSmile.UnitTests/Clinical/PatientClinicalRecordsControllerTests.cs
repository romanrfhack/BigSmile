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
        public async Task GetEncounters_ReturnsNotFound_WhenClinicalRecordDoesNotExist()
        {
            var patientId = Guid.NewGuid();
            var commandService = new Mock<IClinicalRecordCommandService>();
            var queryService = new Mock<IClinicalRecordQueryService>();
            queryService
                .Setup(service => service.GetEncountersByPatientIdAsync(patientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ClinicalEncounterDto>?)null);

            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.GetEncounters(patientId);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateEncounter_ReturnsCreated_WhenCommandServiceSucceeds()
        {
            var patientId = Guid.NewGuid();
            var response = new ClinicalEncounterDto(
                Guid.NewGuid(),
                Guid.NewGuid(),
                patientId,
                new DateTime(2026, 5, 12, 12, 0, 0, DateTimeKind.Utc),
                "Tooth sensitivity.",
                "Treatment",
                36.7m,
                120,
                80,
                72.5m,
                168.0m,
                16,
                78,
                Guid.NewGuid(),
                "Encounter note.",
                DateTime.UtcNow,
                Guid.NewGuid());

            var commandService = new Mock<IClinicalRecordCommandService>();
            commandService
                .Setup(service => service.CreateEncounterAsync(
                    patientId,
                    It.IsAny<CreateClinicalEncounterCommand>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);

            var queryService = new Mock<IClinicalRecordQueryService>();
            var controller = new PatientClinicalRecordsController(commandService.Object, queryService.Object);

            var result = await controller.CreateEncounter(
                patientId,
                new PatientClinicalRecordsController.CreateClinicalEncounterRequest
                {
                    OccurredAtUtc = response.OccurredAtUtc,
                    ChiefComplaint = "Tooth sensitivity.",
                    ConsultationType = "Treatment",
                    TemperatureC = 36.7m,
                    BloodPressureSystolic = 120,
                    BloodPressureDiastolic = 80,
                    WeightKg = 72.5m,
                    HeightCm = 168.0m,
                    RespiratoryRatePerMinute = 16,
                    HeartRateBpm = 78,
                    NoteText = "Encounter note."
                });

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(PatientClinicalRecordsController.GetEncounters), created.ActionName);
            Assert.Same(response, created.Value);
        }

        [Fact]
        public void QuestionnaireAndEncounterEndpoints_RequireClinicalReadAndClinicalWritePolicies()
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
            var getEncountersAuthorize = typeof(PatientClinicalRecordsController)
                .GetMethod(nameof(PatientClinicalRecordsController.GetEncounters))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
                .Cast<AuthorizeAttribute>()
                .Single();
            var createEncounterAuthorize = typeof(PatientClinicalRecordsController)
                .GetMethod(nameof(PatientClinicalRecordsController.CreateEncounter))!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
                .Cast<AuthorizeAttribute>()
                .Single();

            Assert.Equal(AuthorizationPolicies.ClinicalRead, getAuthorize.Policy);
            Assert.Equal(AuthorizationPolicies.ClinicalWrite, putAuthorize.Policy);
            Assert.Equal(AuthorizationPolicies.ClinicalRead, getEncountersAuthorize.Policy);
            Assert.Equal(AuthorizationPolicies.ClinicalWrite, createEncounterAuthorize.Policy);
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

        [Fact]
        public void CreateClinicalEncounterRequest_RejectsInvalidVitals()
        {
            var request = new PatientClinicalRecordsController.CreateClinicalEncounterRequest
            {
                OccurredAtUtc = DateTime.UtcNow,
                ChiefComplaint = "Tooth sensitivity.",
                ConsultationType = "Treatment",
                TemperatureC = 52.0m,
                BloodPressureSystolic = 110,
                BloodPressureDiastolic = 120
            };

            var results = Validate(request);

            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PatientClinicalRecordsController.CreateClinicalEncounterRequest.TemperatureC)));
            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PatientClinicalRecordsController.CreateClinicalEncounterRequest.BloodPressureDiastolic)));
        }

        [Fact]
        public void CreateClinicalEncounterRequest_RejectsInvalidConsultationType()
        {
            var request = new PatientClinicalRecordsController.CreateClinicalEncounterRequest
            {
                OccurredAtUtc = DateTime.UtcNow,
                ChiefComplaint = "Tooth sensitivity.",
                ConsultationType = "FollowUp"
            };

            var results = Validate(request);

            Assert.Contains(results, result => result.MemberNames.Contains(nameof(PatientClinicalRecordsController.CreateClinicalEncounterRequest.ConsultationType)));
        }

        [Fact]
        public void CreateClinicalEncounterRequest_DoesNotAcceptTenantIdOrCreatedByUserIdFromJson()
        {
            var json = """
                {
                  "tenantId": "4bff6ec8-7a0f-4b5c-bcd7-2c60778b3526",
                  "createdByUserId": "5dff6ec8-7a0f-4b5c-bcd7-2c60778b3526",
                  "occurredAtUtc": "2026-05-12T12:00:00Z",
                  "chiefComplaint": "Tooth sensitivity.",
                  "consultationType": "Treatment"
                }
                """;
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            Assert.Throws<JsonException>(() =>
                JsonSerializer.Deserialize<PatientClinicalRecordsController.CreateClinicalEncounterRequest>(json, options));
        }

        private static IReadOnlyCollection<ValidationResult> Validate(object value)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(value, new ValidationContext(value), results, validateAllProperties: true);

            return results;
        }
    }
}
