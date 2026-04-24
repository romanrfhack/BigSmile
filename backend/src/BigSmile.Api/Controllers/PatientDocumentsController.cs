using System.ComponentModel.DataAnnotations;
using BigSmile.Api.Authorization;
using BigSmile.Application.Features.PatientDocuments.Commands;
using BigSmile.Application.Features.PatientDocuments.Dtos;
using BigSmile.Application.Features.PatientDocuments.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.Api.Controllers
{
    [ApiController]
    [Route("api/patients/{patientId:guid}/documents")]
    public class PatientDocumentsController : ControllerBase
    {
        private readonly IPatientDocumentCommandService _patientDocumentCommandService;
        private readonly IPatientDocumentQueryService _patientDocumentQueryService;

        public PatientDocumentsController(
            IPatientDocumentCommandService patientDocumentCommandService,
            IPatientDocumentQueryService patientDocumentQueryService)
        {
            _patientDocumentCommandService = patientDocumentCommandService ?? throw new ArgumentNullException(nameof(patientDocumentCommandService));
            _patientDocumentQueryService = patientDocumentQueryService ?? throw new ArgumentNullException(nameof(patientDocumentQueryService));
        }

        [HttpGet]
        [Authorize(Policy = AuthorizationPolicies.DocumentRead)]
        public async Task<ActionResult<IReadOnlyList<PatientDocumentSummaryDto>>> ListActive(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var documents = await _patientDocumentQueryService.ListActiveByPatientIdAsync(patientId, cancellationToken);
                if (documents is null)
                {
                    return NotFound();
                }

                return Ok(documents);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.DocumentWrite)]
        public async Task<ActionResult<PatientDocumentSummaryDto>> Upload(
            Guid patientId,
            [FromForm] UploadPatientDocumentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var contentStream = request.File!.OpenReadStream();
                var document = await _patientDocumentCommandService.UploadAsync(
                    patientId,
                    request.ToCommand(contentStream),
                    cancellationToken);

                return CreatedAtAction(nameof(ListActive), new { patientId }, document);
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

        [HttpGet("{documentId:guid}/download")]
        [Authorize(Policy = AuthorizationPolicies.DocumentRead)]
        public async Task<IActionResult> Download(
            Guid patientId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var document = await _patientDocumentQueryService.DownloadAsync(patientId, documentId, cancellationToken);
                if (document is null)
                {
                    return NotFound();
                }

                return File(document.ContentStream, document.ContentType, document.OriginalFileName);
            }
            catch (InvalidOperationException exception)
            {
                return BuildValidationProblem(exception.Message);
            }
        }

        [HttpDelete("{documentId:guid}")]
        [Authorize(Policy = AuthorizationPolicies.DocumentWrite)]
        public async Task<IActionResult> Retire(
            Guid patientId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var retired = await _patientDocumentCommandService.RetireAsync(patientId, documentId, cancellationToken);
                if (!retired)
                {
                    return NotFound();
                }

                return NoContent();
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
            ModelState.AddModelError(nameof(PatientDocumentsController), message);
            return ValidationProblem(ModelState);
        }

        public sealed class UploadPatientDocumentRequest : IValidatableObject
        {
            [Required]
            public IFormFile? File { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (File is null)
                {
                    yield return new ValidationResult("Patient document file is required.", new[] { nameof(File) });
                }
            }

            public UploadPatientDocumentCommand ToCommand(Stream contentStream)
            {
                return new UploadPatientDocumentCommand(
                    File?.FileName ?? string.Empty,
                    File?.ContentType ?? string.Empty,
                    File?.Length ?? 0,
                    contentStream);
            }
        }
    }
}
