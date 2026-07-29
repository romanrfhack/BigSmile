using System.Security.Claims;
using BigSmile.Api.Authorization;
using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientIntakes.Dtos;
using BigSmile.Application.Features.PatientIntakes.Services;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalIntakeProblemCodeTests
    {
        [Theory]
        [InlineData(PatientIntakeSaveFailure.ConcurrentConflict, "patient_intake.concurrency_conflict")]
        [InlineData(PatientIntakeSaveFailure.Expired, "patient_intake.expired")]
        public async Task Save_ProvidesStableAdditiveProblemCode(
            PatientIntakeSaveFailure failure,
            string expectedCode)
        {
            var identity = new PatientPortalSessionIdentity(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                1);
            var service = new Mock<IPatientIntakeSelfService>();
            service
                .Setup(candidate => candidate.SaveAsync(
                    It.IsAny<PatientPortalSessionIdentity>(),
                    It.IsAny<SavePatientIntakeDraftCommand>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeSaveResult.Failed(failure));
            var controller = CreateController(service.Object, identity);

            var result = await controller.Save(BuildSaveRequest());

            var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
            var problem = Assert.IsType<ProblemDetails>(conflict.Value);
            Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
            Assert.Equal(expectedCode, problem.Extensions["code"]);
            Assert.DoesNotContain(identity.AccountId.ToString(), problem.Detail!, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(identity.PatientId.ToString(), problem.Detail!, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(identity.TenantId.ToString(), problem.Detail!, StringComparison.OrdinalIgnoreCase);
        }

        private static PatientPortalIntakeController CreateController(
            IPatientIntakeSelfService service,
            PatientPortalSessionIdentity identity)
        {
            var httpContext = new DefaultHttpContext
            {
                TraceIdentifier = "trace-pi2d4"
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, identity.AccountId.ToString()),
                new Claim(BigSmileClaimTypes.TenantId, identity.TenantId.ToString()),
                new Claim(BigSmileClaimTypes.PatientId, identity.PatientId.ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.Patient.ToClaimValue()),
                new Claim(BigSmileClaimTypes.SessionVersion, identity.SessionVersion.ToString())
            }, PatientPortalAuthenticationDefaults.PatientBearerScheme));

            return new PatientPortalIntakeController(service)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private static PatientPortalIntakeController.SavePatientIntakeRequest BuildSaveRequest()
        {
            return new PatientPortalIntakeController.SavePatientIntakeRequest
            {
                Sex = PatientSex.Unspecified.ToString(),
                MaritalStatus = PatientMaritalStatus.Unspecified.ToString(),
                ConcurrencyToken = "rv1.current-token",
                MedicalAnswers = ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys
                    .Select(questionKey => new PatientPortalIntakeController.SavePatientIntakeMedicalAnswerRequest
                    {
                        QuestionKey = questionKey,
                        Answer = ClinicalMedicalAnswerValue.Unknown.ToString()
                    })
                    .ToList()
            };
        }
    }
}
