using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.TreatmentQuotes.Commands;
using BigSmile.Application.Features.TreatmentQuotes.Dtos;
using BigSmile.Application.Features.TreatmentQuotes.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/treatment-plan/quote")]
    public class PatientTreatmentQuotesController : ControllerBase
    {
        private readonly ITreatmentQuoteCommandService _treatmentQuoteCommandService;
        private readonly ITreatmentQuoteQueryService _treatmentQuoteQueryService;

        public PatientTreatmentQuotesController(
            ITreatmentQuoteCommandService treatmentQuoteCommandService,
            ITreatmentQuoteQueryService treatmentQuoteQueryService)
        {
            _treatmentQuoteCommandService = treatmentQuoteCommandService ?? throw new ArgumentNullException(nameof(treatmentQuoteCommandService));
            _treatmentQuoteQueryService = treatmentQuoteQueryService ?? throw new ArgumentNullException(nameof(treatmentQuoteQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.TreatmentQuoteRead)]
        public async Task<ActionResult<TreatmentQuoteDetailDto>> GetByPatientId(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentQuote = await _treatmentQuoteQueryService.GetByPatientIdAsync(patientId, cancellationToken);
                if (treatmentQuote is null)
                {
                    return NotFound();
                }

                return Ok(treatmentQuote);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.TreatmentQuoteWrite)]
        public async Task<ActionResult<TreatmentQuoteDetailDto>> Create(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentQuote = await _treatmentQuoteCommandService.CreateAsync(patientId, cancellationToken);
                return CreatedAtAction(nameof(GetByPatientId), new { patientId }, treatmentQuote);
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

        [HttpPut("items/{quoteItemId:guid}/price")]
        [Authorize(Policy = AuthorizationPolicies.TreatmentQuoteWrite)]
        public async Task<ActionResult<TreatmentQuoteDetailDto>> UpdateItemUnitPrice(
            Guid patientId,
            Guid quoteItemId,
            [FromBody] UpdateTreatmentQuoteItemPriceRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentQuote = await _treatmentQuoteCommandService.UpdateItemUnitPriceAsync(
                    patientId,
                    quoteItemId,
                    request.ToCommand(),
                    cancellationToken);

                if (treatmentQuote is null)
                {
                    return NotFound();
                }

                return Ok(treatmentQuote);
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
        [Authorize(Policy = AuthorizationPolicies.TreatmentQuoteWrite)]
        public async Task<ActionResult<TreatmentQuoteDetailDto>> ChangeStatus(
            Guid patientId,
            [FromBody] ChangeTreatmentQuoteStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var treatmentQuote = await _treatmentQuoteCommandService.ChangeStatusAsync(
                    patientId,
                    request.ToCommand(),
                    cancellationToken);

                if (treatmentQuote is null)
                {
                    return NotFound();
                }

                return Ok(treatmentQuote);
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
            ModelState.AddModelError(nameof(PatientTreatmentQuotesController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class UpdateTreatmentQuoteItemPriceRequest : IValidatableObject
        {
            public decimal UnitPrice { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (UnitPrice < 0)
                {
                    yield return new ValidationResult(
                        "Treatment quote item unit price must be greater than or equal to zero.",
                        new[] { nameof(UnitPrice) });
                }
            }

            public UpdateTreatmentQuoteItemPriceCommand ToCommand()
            {
                return new UpdateTreatmentQuoteItemPriceCommand(UnitPrice);
            }
        }

        public sealed class ChangeTreatmentQuoteStatusRequest : IValidatableObject
        {
            [Required]
            public string Status { get; set; } = string.Empty;

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Status))
                {
                    yield return new ValidationResult("Treatment quote status is required.", new[] { nameof(Status) });
                }
            }

            public ChangeTreatmentQuoteStatusCommand ToCommand()
            {
                return new ChangeTreatmentQuoteStatusCommand(Status);
            }
        }
    }
}
