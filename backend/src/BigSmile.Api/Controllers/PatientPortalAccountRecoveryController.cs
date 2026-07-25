using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientPortalAuthentication.Commands;
using BigSmile.Application.Features.PatientPortalInvitations.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/portal-account/recovery")]
    [Authorize(Policy = AuthorizationPolicies.PatientPortalAccountRecover)]
    public sealed class PatientPortalAccountRecoveryController : ControllerBase
    {
        private readonly IPatientPortalRecoveryService _recoveryService;

        public PatientPortalAccountRecoveryController(IPatientPortalRecoveryService recoveryService)
        {
            _recoveryService = recoveryService ?? throw new ArgumentNullException(nameof(recoveryService));
        }

        [HttpPost]
        public async Task<ActionResult<IssuedPatientPortalInvitationDto>> Start(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var invitation = await _recoveryService.StartRecoveryAsync(
                    patientId,
                    GetCorrelationId(),
                    cancellationToken);
                if (invitation is null)
                {
                    return NotFound();
                }

                SetNoStoreHeaders();
                return Ok(invitation);
            }
            catch (InvalidOperationException exception)
            {
                return Conflict(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Patient portal recovery conflict.",
                    Detail = exception.Message
                });
            }
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
    }
}
