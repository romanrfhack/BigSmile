using BigSmile.Api.Authorization;
using BigSmile.Api.Controllers;
using BigSmile.Application.Features.PatientIntakeAuthentication.Commands;
using BigSmile.Application.Features.PatientIntakeAuthentication.Dtos;
using BigSmile.Application.Interfaces.Security;
using BigSmile.SharedKernel.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace BigSmile.UnitTests.PatientPortal
{
    public sealed class PatientIntakeAuthenticationControllerTests
    {
        [Fact]
        public async Task Activate_ReturnsIntakeTokenWithNoStoreHeaders_WhenSuccessful()
        {
            var response = BuildResponse();
            var authentication = new Mock<IPatientIntakePublicAuthenticationService>();
            authentication
                .Setup(service => service.ActivateAsync(
                    It.IsAny<ActivatePatientIntakeAccountCommand>(),
                    "trace-intake-auth",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeActivationResult.Success(response));
            var controller = CreateController(
                authentication.Object,
                Mock.Of<IPatientIntakeSessionService>());

            var result = await controller.Activate(
                new PatientIntakeAuthenticationController.ActivatePatientIntakeAccountRequest
                {
                    AccessToken = "one-time-waiting-room-token",
                    LoginName = "new.patient",
                    Password = "A sufficiently long waiting-room password."
                });

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response, ok.Value);
            Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
            Assert.Equal("no-cache", controller.Response.Headers.Pragma.ToString());
        }

        [Fact]
        public async Task Activate_UsesSameGenericUnauthorizedShape_ForInvalidCredential()
        {
            var authentication = new Mock<IPatientIntakePublicAuthenticationService>();
            authentication
                .Setup(service => service.ActivateAsync(
                    It.IsAny<ActivatePatientIntakeAccountCommand>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(PatientIntakeActivationResult.Failed(
                    PatientIntakeActivationFailure.InvalidActivation));
            var controller = CreateController(
                authentication.Object,
                Mock.Of<IPatientIntakeSessionService>());

            var result = await controller.Activate(
                new PatientIntakeAuthenticationController.ActivatePatientIntakeAccountRequest
                {
                    AccessToken = "invalid-secret-token",
                    LoginName = "secret.login",
                    Password = "A sufficiently long waiting-room password."
                });

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
            var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
            Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
            Assert.DoesNotContain(
                "invalid-secret-token",
                problem.Detail!,
                StringComparison.Ordinal);
            Assert.DoesNotContain(
                "secret.login",
                problem.Detail!,
                StringComparison.Ordinal);
        }

        [Fact]
        public async Task GetCurrent_UsesOnlyIntakeIdentity()
        {
            var response = BuildResponse();
            var session = new Mock<IPatientIntakeSessionService>();
            session
                .Setup(service => service.GetCurrentAsync(
                    It.Is<PatientIntakeSessionIdentity>(identity =>
                        identity.AccountId == response.Current.AccountId &&
                        identity.IntakeId == response.Current.IntakeId),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response.Current);
            var controller = CreateController(
                Mock.Of<IPatientIntakePublicAuthenticationService>(),
                session.Object);
            controller.ControllerContext.HttpContext.User = BuildPrincipal(response.Current);

            var result = await controller.GetCurrent();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(response.Current, ok.Value);
            Assert.Equal("no-store", controller.Response.Headers.CacheControl.ToString());
        }

        private static PatientIntakeAuthenticationController CreateController(
            IPatientIntakePublicAuthenticationService authentication,
            IPatientIntakeSessionService session)
        {
            return new PatientIntakeAuthenticationController(authentication, session)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        TraceIdentifier = "trace-intake-auth"
                    }
                }
            };
        }

        private static PatientIntakeAuthenticationResponseDto BuildResponse()
        {
            return new PatientIntakeAuthenticationResponseDto(
                "patient-intake-jwt",
                DateTime.UtcNow.AddHours(1),
                new CurrentPatientIntakeSessionDto(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "tenant-a",
                    "new.patient",
                    1));
        }

        private static ClaimsPrincipal BuildPrincipal(
            CurrentPatientIntakeSessionDto current)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, current.AccountId.ToString()),
                new Claim(BigSmileClaimTypes.TenantId, Guid.NewGuid().ToString()),
                new Claim(BigSmileClaimTypes.IntakeId, current.IntakeId.ToString()),
                new Claim(BigSmileClaimTypes.Scope, AccessScope.PatientIntake.ToClaimValue()),
                new Claim(BigSmileClaimTypes.SessionVersion, current.SessionVersion.ToString())
            }, PatientPortalAuthenticationDefaults.PatientBearerScheme));
        }
    }
}
