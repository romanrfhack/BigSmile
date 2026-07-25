using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientPortalAuthentication.Commands;
using BigSmile.Application.Features.PatientPortalAuthentication.Dtos;
using BigSmile.Application.Interfaces.Security;
using BigSmile.SharedKernel.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientPortalAuthenticationControllerTests
    {
        [Fact]
        public async Task Activate_ReturnsTokenWithNoStoreHeaders_WhenSuccessful()
        {
            var response = BuildAuthenticationResponse();
            var authenticationService = new Mock<IPatientPortalPublicAuthenticationService>();
            authenticationService
                .Setup(service => service.ActivateAsync(
                    It.IsAny<ActivatePatientPortalAccountCommand>(),
                    "trace-1",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientPortalActivationResult.Success(response));
            var controller = CreateController(authenticationService.Object, Mock.Of<IPatientPortalSessionService>());

            var result = await controller.Activate(new PatientPortalAuthenticationController.ActivatePatientPortalAccountRequest
            {
                ActivationToken = "one-time-token",
                LoginName = "patient.login",
                Password = "A sufficiently long patient password."
            });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
            Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
            Assert.Equal("no-cache", controller.Response.Headers.Pragma.ToString());
        }

        [Fact]
        public async Task Activate_UsesGenericUnauthorizedResponse_ForInvalidCredential()
        {
            var authenticationService = new Mock<IPatientPortalPublicAuthenticationService>();
            authenticationService
                .Setup(service => service.ActivateAsync(
                    It.IsAny<ActivatePatientPortalAccountCommand>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientPortalActivationResult.Failed(
                    PatientPortalActivationFailure.InvalidActivation));
            var controller = CreateController(authenticationService.Object, Mock.Of<IPatientPortalSessionService>());

            var result = await controller.Activate(new PatientPortalAuthenticationController.ActivatePatientPortalAccountRequest
            {
                ActivationToken = "invalid-token",
                LoginName = "patient.login",
                Password = "A sufficiently long patient password."
            });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
            Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
            Assert.DoesNotContain("invalid-token", problem.Detail!, StringComparison.Ordinal);
            Assert.DoesNotContain("patient.login", problem.Detail!, StringComparison.Ordinal);
        }

        [Fact]
        public async Task Login_ReturnsSameGenericUnauthorizedShape_WhenAuthenticationFails()
        {
            var authenticationService = new Mock<IPatientPortalPublicAuthenticationService>();
            authenticationService
                .Setup(service => service.LoginAsync(
                    It.IsAny<LoginPatientPortalAccountCommand>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((PatientPortalAuthenticationResponseDto?)null);
            var controller = CreateController(authenticationService.Object, Mock.Of<IPatientPortalSessionService>());

            var result = await controller.Login(
                "unknown-realm",
                new PatientPortalAuthenticationController.LoginPatientPortalAccountRequest
                {
                    LoginName = "unknown-login",
                    Password = "unknown-password"
                });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
            Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
            Assert.DoesNotContain("unknown-realm", problem.Detail!, StringComparison.Ordinal);
            Assert.DoesNotContain("unknown-login", problem.Detail!, StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetCurrent_UsesOnlyBoundedPatientClaims()
        {
            var current = BuildAuthenticationResponse().Current;
            var sessionService = new Mock<IPatientPortalSessionService>();
            sessionService
                .Setup(service => service.GetCurrentAsync(
                    It.Is<PatientPortalSessionIdentity>(identity =>
                        identity.AccountId == current.AccountId &&
                        identity.PatientId == current.PatientId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(current);
            var controller = CreateController(Mock.Of<IPatientPortalPublicAuthenticationService>(), sessionService.Object);
            controller.ControllerContext.HttpContext.User = BuildPatientPrincipal(current);

            var result = await controller.GetCurrent();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(current, ok.Value);
        }

        private static PatientPortalAuthenticationController CreateController(
            IPatientPortalPublicAuthenticationService authenticationService,
            IPatientPortalSessionService sessionService)
        {
            return new PatientPortalAuthenticationController(authenticationService, sessionService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        TraceIdentifier = "trace-1"
                    }
                }
            };
        }

        private static PatientPortalAuthenticationResponseDto BuildAuthenticationResponse()
        {
            return new PatientPortalAuthenticationResponseDto(
                "patient-jwt",
                DateTime.UtcNow.AddHours(1),
                new CurrentPatientPortalSessionDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "tenant-a",
                    "patient.login",
                    1));
        }

        private static ClaimsPrincipal BuildPatientPrincipal(CurrentPatientPortalSessionDto current)
        {
            var tenantId = Guid.NewGuid();
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, current.AccountId.ToString()),
                new Claim(BigSmileClaimTypes.TenantId, tenantId.ToString()),
                new Claim(BigSmileClaimTypes.PatientId, current.PatientId.ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.Patient.ToClaimValue()),
                new Claim(BigSmileClaimTypes.SessionVersion, current.SessionVersion.ToString())
            }, "PatientPortalBearer"));
        }
    }
}
