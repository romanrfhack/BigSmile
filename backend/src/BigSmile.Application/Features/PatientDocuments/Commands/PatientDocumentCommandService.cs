using System.IO;
using BigSmile.Application.Features.PatientDocuments.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Storage;
using BigSmile.Domain.Entities;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientDocuments.Commands
{
    public sealed record UploadPatientDocumentCommand(
        string OriginalFileName,
        string ContentType,
        long SizeBytes,
        Stream ContentStream);

    public interface IPatientDocumentCommandService
    {
        Task<PatientDocumentSummaryDto> UploadAsync(
            Guid patientId,
            UploadPatientDocumentCommand command,
            CancellationToken cancellationToken = default);

        Task<bool> RetireAsync(
            Guid patientId,
            Guid documentId,
            CancellationToken cancellationToken = default);
    }

    public sealed class PatientDocumentCommandService : IPatientDocumentCommandService
    {
        private readonly IPatientDocumentRepository _patientDocumentRepository;
        private readonly IPatientDocumentBinaryStore _patientDocumentBinaryStore;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public PatientDocumentCommandService(
            IPatientDocumentRepository patientDocumentRepository,
            IPatientDocumentBinaryStore patientDocumentBinaryStore,
            IPatientRepository patientRepository,
            ITenantContext tenantContext)
        {
            _patientDocumentRepository = patientDocumentRepository ?? throw new ArgumentNullException(nameof(patientDocumentRepository));
            _patientDocumentBinaryStore = patientDocumentBinaryStore ?? throw new ArgumentNullException(nameof(patientDocumentBinaryStore));
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        }

        public async Task<PatientDocumentSummaryDto> UploadAsync(
            Guid patientId,
            UploadPatientDocumentCommand command,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var originalFileName = NormalizeOriginalFileName(command.OriginalFileName);
            var storageKey = BuildStorageKey(tenantId, patientId, command.ContentType);
            var patientDocument = new PatientDocument(
                tenantId,
                patientId,
                originalFileName,
                command.ContentType,
                command.SizeBytes,
                storageKey,
                actorUserId);

            try
            {
                await _patientDocumentBinaryStore.SaveAsync(storageKey, command.ContentStream, cancellationToken);
                await _patientDocumentRepository.AddAsync(patientDocument, cancellationToken);
            }
            catch
            {
                await _patientDocumentBinaryStore.DeleteIfExistsAsync(storageKey, cancellationToken);
                throw;
            }

            return patientDocument.ToSummaryDto();
        }

        public async Task<bool> RetireAsync(
            Guid patientId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var tenantId = GetRequiredTenantId();
            var actorUserId = GetRequiredUserId();
            var patient = await GetRequiredPatientAsync(patientId, cancellationToken);

            EnsurePatientBelongsToTenant(patient, tenantId);

            var patientDocument = await _patientDocumentRepository.GetActiveByIdAsync(patientId, documentId, cancellationToken);
            if (patientDocument is null)
            {
                return false;
            }

            patientDocument.Retire(actorUserId);
            await _patientDocumentRepository.UpdateAsync(patientDocument, cancellationToken);
            return true;
        }

        private async Task<Patient> GetRequiredPatientAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                throw new InvalidOperationException("The requested patient is not available in the current tenant scope.");
            }

            return patient;
        }

        private static void EnsurePatientBelongsToTenant(Patient patient, Guid tenantId)
        {
            if (patient.TenantId != tenantId)
            {
                throw new InvalidOperationException("Patient documents can only reference patients from the current tenant.");
            }
        }

        private Guid GetRequiredTenantId()
        {
            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient document operations require a resolved tenant context.");
            }

            return tenantId;
        }

        private Guid GetRequiredUserId()
        {
            var userIdValue = _tenantContext.GetUserId();
            if (!Guid.TryParse(userIdValue, out var userId) || userId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient document write operations require a resolved user context.");
            }

            return userId;
        }

        private static string NormalizeOriginalFileName(string originalFileName)
        {
            var normalized = Path.GetFileName(originalFileName?.Trim() ?? string.Empty);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new ArgumentException("Patient document original file name is required.", nameof(originalFileName));
            }

            return normalized;
        }

        private static string BuildStorageKey(Guid tenantId, Guid patientId, string contentType)
        {
            var extension = contentType.Trim().ToLowerInvariant() switch
            {
                "application/pdf" => ".pdf",
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => string.Empty
            };

            return $"patients/{tenantId:N}/{patientId:N}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        }
    }
}
