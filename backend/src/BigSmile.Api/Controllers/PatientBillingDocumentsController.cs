using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.BillingDocuments.Commands;
using BigSmile.Application.Features.BillingDocuments.Dtos;
using BigSmile.Application.Features.BillingDocuments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/treatment-plan/quote/billing")]
    public class PatientBillingDocumentsController : ControllerBase
    {
        private readonly IBillingDocumentCommandService _billingDocumentCommandService;
        private readonly IBillingDocumentQueryService _billingDocumentQueryService;

        public PatientBillingDocumentsController(
            IBillingDocumentCommandService billingDocumentCommandService,
            IBillingDocumentQueryService billingDocumentQueryService)
        {
            _billingDocumentCommandService = billingDocumentCommandService ?? throw new ArgumentNullException(nameof(billingDocumentCommandService));
            _billingDocumentQueryService = billingDocumentQueryService ?? throw new ArgumentNullException(nameof(billingDocumentQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.BillingRead)]
        public async Task<ActionResult<BillingDocumentDetailDto>> GetByPatientId(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var billingDocument = await _billingDocumentQueryService.GetByPatientIdAsync(patientId, cancellationToken);
                if (billingDocument is null)
                {
                    return NotFound();
                }

                return Ok(billingDocument);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.BillingWrite)]
        public async Task<ActionResult<BillingDocumentDetailDto>> Create(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var billingDocument = await _billingDocumentCommandService.CreateAsync(patientId, cancellationToken);
                return CreatedAtAction(nameof(GetByPatientId), new { patientId }, billingDocument);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPut("status")]
        [Authorize(Policy = AuthorizationPolicies.BillingWrite)]
        public async Task<ActionResult<BillingDocumentDetailDto>> ChangeStatus(
            Guid patientId,
            [FromBody] ChangeBillingDocumentStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var billingDocument = await _billingDocumentCommandService.ChangeStatusAsync(
                    patientId,
                    request.ToCommand(),
                    cancellationToken);

                if (billingDocument is null)
                {
                    return NotFound();
                }

                return Ok(billingDocument);
            }
            catch (ArgumentException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        private ActionResult BuildValidationProblem(string message)
        {
            ModelState.AddModelError(nameof(PatientBillingDocumentsController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class ChangeBillingDocumentStatusRequest : IValidatableObject
        {
            [Required]
            public string Status { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Status))
                {
                    yield return new ValidationResult("Billing document status is required.", new[] { nameof(Status) });
                }
            }

            public ChangeBillingDocumentStatusCommand ToCommand()
            {
                return new ChangeBillingDocumentStatusCommand(Status);
            }
        }
    }
}
