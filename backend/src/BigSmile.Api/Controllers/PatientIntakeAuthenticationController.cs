using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientIntakeAuthentication.Commands;
using BigSmile.Application.Features.PatientIntakeAuthentication.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patient-portal/intake-auth")]
    public sealed class PatientIntakeAuthenticationController : ControllerBase
    {
        public const string ActivationRateLimitPolicy = "patient-intake-activation";
        public const string LoginRateLimitPolicy = "patient-intake-login";

        private readonly IPatientIntakePublicAuthenticationService _authenticationService;
        private readonly IPatientIntakeSessionService _sessionService;

        public PatientIntakeAuthenticationController(
            IPatientIntakePublicAuthenticationService authenticationService,
            IPatientIntakeSessionService sessionService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        [HttpPost("activate")]
        [AllowAnonymous]
        [EnableRateLimiting(ActivationRateLimitPolicy)]
        public async Task<ActionResult<PatientIntakeAuthenticationResponseDto>> Activate(
            [FromBody] ActivatePatientIntakeAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _authenticationService.ActivateAsync(
                    new ActivatePatientIntakeAccountCommand(
                        request.AccessToken,
                        request.LoginName,
                        request.Password),
                    GetCorrelationId(),
                    cancellationToken);

                if (result.Succeeded)
                {
                    SetNoStoreHeaders();
                    return Ok(result.Authentication);
                }

                return result.Failure switch
                {
                    PatientIntakeActivationFailure.LoginNameUnavailable => BuildGenericConflict(),
                    PatientIntakeActivationFailure.ConcurrentConflict => BuildGenericConflict(),
                    _ => BuildGenericUnauthorized()
                };
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(nameof(ActivatePatientIntakeAccountRequest), exception.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPost("realms/{tenantSubdomain}/login")]
        [AllowAnonymous]
        [EnableRateLimiting(LoginRateLimitPolicy)]
        public async Task<ActionResult<PatientIntakeAuthenticationResponseDto>> Login(
            string tenantSubdomain,
            [FromBody] LoginPatientIntakeAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _authenticationService.LoginAsync(
                new LoginPatientIntakeAccountCommand(
                    tenantSubdomain,
                    request.LoginName,
                    request.Password),
                GetCorrelationId(),
                cancellationToken);

            if (result is null)
            {
                return BuildGenericUnauthorized();
            }

            SetNoStoreHeaders();
            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize(Policy = PatientPortalAuthenticationDefaults.IntakeOnlyPolicy)]
        public async Task<ActionResult<CurrentPatientIntakeSessionDto>> GetCurrent(
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();
            if (!PatientPortalClaims.TryGetIntakeSessionIdentity(User, out var identity))
            {
                return Unauthorized();
            }

            var current = await _sessionService.GetCurrentAsync(
                identity,
                cancellationToken);
            return current is null ? Unauthorized() : Ok(current);
        }

        [HttpPost("logout")]
        [Authorize(Policy = PatientPortalAuthenticationDefaults.IntakeOnlyPolicy)]
        public async Task<IActionResult> Logout(
            CancellationToken cancellationToken = default)
        {
            SetNoStoreHeaders();
            if (!PatientPortalClaims.TryGetIntakeSessionIdentity(User, out var identity))
            {
                return Unauthorized();
            }

            var revoked = await _sessionService.RevokeCurrentSessionsAsync(
                identity,
                GetCorrelationId(),
                cancellationToken);
            return revoked ? NoContent() : Unauthorized();
        }

        private ObjectResult BuildGenericUnauthorized()
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Patient intake authentication failed.",
                Detail = "The supplied patient intake credential is not valid."
            });
        }

        private ObjectResult BuildGenericConflict()
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Patient intake activation conflict.",
                Detail = "Patient intake activation could not be completed with the supplied data."
            });
        }

        private string GetCorrelationId()
        {
            return string.IsNullOrWhiteSpace(HttpContext.TraceIdentifier)
                ? Guid.NewGuid().ToString("N")
                : HttpContext.TraceIdentifier;
        }

        private void SetNoStoreHeaders()
        {
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
            Response.Headers.Expires = "0";
        }

        public sealed class ActivatePatientIntakeAccountRequest
        {
            [Required]
            [MaxLength(256)]
            public string AccessToken { get; set; } = string.Empty;

            [Required]
            [MinLength(3)]
            [MaxLength(200)]
            public string LoginName { get; set; } = string.Empty;

            [Required]
            [MaxLength(512)]
            public string Password { get; set; } = string.Empty;
        }

        public sealed class LoginPatientIntakeAccountRequest
        {
            [Required]
            [MinLength(3)]
            [MaxLength(200)]
            public string LoginName { get; set; } = string.Empty;

            [Required]
            [MaxLength(512)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
