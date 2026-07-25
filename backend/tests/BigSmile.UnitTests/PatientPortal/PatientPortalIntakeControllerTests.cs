using System.Reflection;
using System.Security.Claims;
using BigSmile.Api.Authorization;
using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientIntakes.Dtos;
using BigSmile.Application.Features.PatientIntakes.Services;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalIntakeControllerTests
    {
        [Fact]
        public async Task GetCurrent_ReturnsOnlyCurrentSessionIntakeWithNoStoreHeaders()
        {
            var identity = BuildIdentity();
            var intake = BuildIntakeDto();
            var service = new Mock<IPatientIntakeSelfService>();
            service
                .Setup(candidate => candidate.GetCurrentAsync(
                    It.Is<PatientPortalSessionIdentity>(value => value == identity),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeReadResult.Success(intake));
            var controller = CreateController(service.Object, identity);

            var result = await controller.GetCurrent();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(intake, ok.Value);
            AssertNoStore(controller.Response.Headers);
        }

        [Fact]
        public async Task Create_ReturnsCreatedWithoutAcceptingOwnershipIdentifiers()
        {
            var identity = BuildIdentity();
            var intake = BuildIntakeDto();
            var service = new Mock<IPatientIntakeSelfService>();
            service
                .Setup(candidate => candidate.CreateAsync(
                    It.Is<PatientPortalSessionIdentity>(value => value == identity),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeCreateResult.Success(intake));
            var controller = CreateController(service.Object, identity);

            var result = await controller.Create();

            var created = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, created.StatusCode);
            Assert.Same(intake, created.Value);
            AssertNoStore(controller.Response.Headers);
        }

        [Fact]
        public async Task Save_MapsCompleteSnapshotAndReturnsChangedFlag()
        {
            var identity = BuildIdentity();
            var intake = BuildIntakeDto() with
            {
                CurrentRevisionNumber = 1,
                ConcurrencyToken = "fb1.updated-token",
                ReasonForVisit = "Pain while chewing"
            };
            var service = new Mock<IPatientIntakeSelfService>();
            service
                .Setup(candidate => candidate.SaveAsync(
                    It.Is<PatientPortalSessionIdentity>(value => value == identity),
                    It.Is<SavePatientIntakeDraftCommand>(command =>
                        command.FirstName == "Ana" &&
                        command.ReasonForVisit == "Pain while chewing" &&
                        command.MedicalAnswers.Count == 39 &&
                        command.ConcurrencyToken == "fb1.current-token"),
                    "trace-intake-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeSaveResult.Success(intake, changed: true));
            var controller = CreateController(service.Object, identity);
            var request = BuildSaveRequest();

            var result = await controller.Save(request);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<PatientIntakeSaveResponseDto>(ok.Value);
            Assert.True(response.Changed);
            Assert.Same(intake, response.Intake);
            AssertNoStore(controller.Response.Headers);
        }

        [Theory]
        [InlineData(PatientIntakeSaveFailure.ConcurrentConflict)]
        [InlineData(PatientIntakeSaveFailure.Expired)]
        public async Task Save_ReturnsBoundedConflictWithoutOwnershipDisclosure(
            PatientIntakeSaveFailure failure)
        {
            var identity = BuildIdentity();
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
            Assert.DoesNotContain(identity.AccountId.ToString(), problem.Detail!, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(identity.PatientId.ToString(), problem.Detail!, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain(identity.TenantId.ToString(), problem.Detail!, StringComparison.OrdinalIgnoreCase);
            AssertNoStore(controller.Response.Headers);
        }

        [Fact]
        public async Task InvalidOrStaffLikeClaimsAreRejectedBeforeServiceExecution()
        {
            var service = new Mock<IPatientIntakeSelfService>(MockBehavior.Strict);
            var controller = CreateController(service.Object, identity: null);
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.Tenant.ToClaimValue()),
                new Claim(BigSmileClaimTypes.Permission, "patient.write")
            }, "Bearer"));

            var getResult = await controller.GetCurrent();
            var createResult = await controller.Create();
            var saveResult = await controller.Save(BuildSaveRequest());

            Assert.IsType<UnauthorizedResult>(getResult.Result);
            Assert.IsType<UnauthorizedResult>(createResult.Result);
            Assert.IsType<UnauthorizedResult>(saveResult.Result);
            service.VerifyNoOtherCalls();
        }

        [Fact]
        public void SaveRequest_NullMedicalAnswersProducesControlledValidation()
        {
            var request = BuildSaveRequest();
            request.MedicalAnswers = null!;
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                request,
                new System.ComponentModel.DataAnnotations.ValidationContext(request),
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(validationResults, result =>
                result.MemberNames.Contains(nameof(request.MedicalAnswers), StringComparer.Ordinal));
            Assert.Throws<ArgumentException>(() => request.ToCommand());
        }

        [Fact]
        public void ControllerAndRequestContractPreserveSelfOnlyIdLessBoundary()
        {
            var authorize = typeof(PatientPortalIntakeController)
                .GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(authorize);
            Assert.Equal(
                PatientPortalAuthenticationDefaults.PatientIntakeSelfPolicy,
                authorize!.Policy);

            var route = typeof(PatientPortalIntakeController)
                .GetCustomAttribute<RouteAttribute>();
            Assert.Equal("api/patient-portal/intake", route!.Template);

            var requestProperties = typeof(PatientPortalIntakeController.SavePatientIntakeRequest)
                .GetProperties()
                .Select(property => property.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain("TenantId", requestProperties);
            Assert.DoesNotContain("PatientPortalAccountId", requestProperties);
            Assert.DoesNotContain("AccountId", requestProperties);
            Assert.DoesNotContain("PatientId", requestProperties);
            Assert.DoesNotContain("PatientIntakeId", requestProperties);
            Assert.DoesNotContain("IntakeId", requestProperties);

            var methods = typeof(PatientPortalIntakeController)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Assert.Contains(methods, method => method.Name == nameof(PatientPortalIntakeController.GetCurrent) &&
                                                method.GetCustomAttribute<HttpGetAttribute>() is not null);
            Assert.Contains(methods, method => method.Name == nameof(PatientPortalIntakeController.Create) &&
                                                method.GetCustomAttribute<HttpPostAttribute>() is not null);
            Assert.Contains(methods, method => method.Name == nameof(PatientPortalIntakeController.Save) &&
                                                method.GetCustomAttribute<HttpPutAttribute>() is not null);
        }

        private static PatientPortalIntakeController CreateController(
            IPatientIntakeSelfService service,
            PatientPortalSessionIdentity? identity)
        {
            var httpContext = new DefaultHttpContext
            {
                TraceIdentifier = "trace-intake-1"
            };
            if (identity is not null)
            {
                httpContext.User = BuildPatientPrincipal(identity);
            }

            return new PatientPortalIntakeController(service)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private static PatientPortalSessionIdentity BuildIdentity()
        {
            return new PatientPortalSessionIdentity(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                1);
        }

        private static ClaimsPrincipal BuildPatientPrincipal(
            PatientPortalSessionIdentity identity)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, identity.AccountId.ToString()),
                new Claim(BigSmileClaimTypes.TenantId, identity.TenantId.ToString()),
                new Claim(BigSmileClaimTypes.PatientId, identity.PatientId.ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.Patient.ToClaimValue()),
                new Claim(BigSmileClaimTypes.SessionVersion, identity.SessionVersion.ToString())
            }, PatientPortalAuthenticationDefaults.PatientBearerScheme));
        }

        private static PatientPortalIntakeController.SavePatientIntakeRequest BuildSaveRequest()
        {
            return new PatientPortalIntakeController.SavePatientIntakeRequest
            {
                FirstName = "Ana",
                LastName = "Lopez",
                DateOfBirth = new DateOnly(1991, 2, 14),
                Sex = PatientSex.Female.ToString(),
                Occupation = "Designer",
                MaritalStatus = PatientMaritalStatus.Single.ToString(),
                ReferredBy = "Friend",
                PreferredPhone = "555-0100",
                MobilePhone = "555-0200",
                Email = "ana@example.com",
                ResponsiblePartyName = "Laura Lopez",
                ResponsiblePartyRelationship = "Mother",
                ResponsiblePartyPhone = "555-0101",
                ReasonForVisit = "Pain while chewing",
                ConcurrencyToken = "fb1.current-token",
                MedicalAnswers = ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys
                    .Select(questionKey => new PatientPortalIntakeController.SavePatientIntakeMedicalAnswerRequest
                    {
                        QuestionKey = questionKey,
                        Answer = ClinicalMedicalAnswerValue.Unknown.ToString()
                    })
                    .ToList()
            };
        }

        private static PatientIntakeDto BuildIntakeDto()
        {
            return new PatientIntakeDto(
                PatientIntakeOrigin.ExistingPatientPortal.ToString(),
                PatientIntakeStatus.Draft.ToString(),
                "Ana",
                "Lopez",
                new DateOnly(1991, 2, 14),
                PatientSex.Female.ToString(),
                "Designer",
                PatientMaritalStatus.Single.ToString(),
                "Friend",
                "555-0100",
                null,
                null,
                null,
                "ana@example.com",
                "Laura Lopez",
                "Mother",
                "555-0101",
                null,
                ClinicalMedicalQuestionnaireCatalog.AllowedQuestionKeys
                    .Select(questionKey => new PatientIntakeMedicalAnswerDto(
                        questionKey,
                        ClinicalMedicalAnswerValue.Unknown.ToString(),
                        null))
                    .ToArray(),
                0,
                "fb1.current-token",
                DateTime.UtcNow,
                DateTime.UtcNow,
                null,
                DateTime.UtcNow.AddDays(30));
        }

        private static void AssertNoStore(IHeaderDictionary headers)
        {
            Assert.Equal("no-store", headers.CacheControl.ToString());
            Assert.Equal("no-cache", headers.Pragma.ToString());
            Assert.Equal("0", headers.Expires.ToString());
        }
    }
}
