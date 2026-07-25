using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientPortalAuthentication.Commands;
using BigSmile.Application.Features.PatientPortalAuthentication.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patient-portal/auth")]
    public sealed class PatientPortalAuthenticationController : ControllerBase
    {
        public const string ActivationRateLimitPolicy = "patient-portal-activation";
        public const string LoginRateLimitPolicy = "patient-portal-login";

        private readonly IPatientPortalPublicAuthenticationService _authenticationService;
        private readonly IPatientPortalSessionService _sessionService;

        public PatientPortalAuthenticationController(
            IPatientPortalPublicAuthenticationService authenticationService,
            IPatientPortalSessionService sessionService)
        {
            _authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        }

        [HttpPost("activate")]
        [AllowAnonymous]
        [EnableRateLimiting(ActivationRateLimitPolicy)]
        public async Task<ActionResult<PatientPortalAuthenticationResponseDto>> Activate(
            [FromBody] ActivatePatientPortalAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _authenticationService.ActivateAsync(
                    new ActivatePatientPortalAccountCommand(
                        request.ActivationToken,
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
                    PatientPortalActivationFailure.LoginNameUnavailable => BuildGenericConflict(),
                    PatientPortalActivationFailure.ConcurrentConflict => BuildGenericConflict(),
                    _ => BuildGenericUnauthorized()
                };
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(nameof(ActivatePatientPortalAccountRequest), exception.Message);
                return ValidationProblem(ModelState);
            }
        }

        [HttpPost("realms/{tenantSubdomain}/login")]
        [AllowAnonymous]
        [EnableRateLimiting(LoginRateLimitPolicy)]
        public async Task<ActionResult<PatientPortalAuthenticationResponseDto>> Login(
            string tenantSubdomain,
            [FromBody] LoginPatientPortalAccountRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _authenticationService.LoginAsync(
                new LoginPatientPortalAccountCommand(
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
        [Authorize(Policy = PatientPortalAuthenticationDefaults.PatientSelfPolicy)]
        public async Task<ActionResult<CurrentPatientPortalSessionDto>> GetCurrent(
            CancellationToken cancellationToken = default)
        {
            if (!PatientPortalClaims.TryGetSessionIdentity(User, out var identity))
            {
                return Unauthorized();
            }

            var current = await _sessionService.GetCurrentAsync(identity, cancellationToken);
            return current is null ? Unauthorized() : Ok(current);
        }

        [HttpPost("logout")]
        [Authorize(Policy = PatientPortalAuthenticationDefaults.PatientSelfPolicy)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
        {
            if (!PatientPortalClaims.TryGetSessionIdentity(User, out var identity))
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
                Title = "Patient portal authentication failed.",
                Detail = "The supplied patient portal credential is not valid."
            });
        }

        private ObjectResult BuildGenericConflict()
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Patient portal activation conflict.",
                Detail = "Patient portal activation could not be completed with the supplied data."
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

        public sealed class ActivatePatientPortalAccountRequest
        {
            [Required]
            [MaxLength(256)]
            public string ActivationToken { get; set; } = string.Empty;

            [Required]
            [MinLength(3)]
            [MaxLength(200)]
            public string LoginName { get; set; } = string.Empty;

            [Required]
            [MinLength(12)]
            [MaxLength(128)]
            public string Password { get; set; } = string.Empty;
        }

        public sealed class LoginPatientPortalAccountRequest
        {
            [Required]
            [MinLength(3)]
            [MaxLength(200)]
            public string LoginName { get; set; } = string.Empty;

            [Required]
            [MaxLength(128)]
            public string Password { get; set; } = string.Empty;
        }
    }
}
