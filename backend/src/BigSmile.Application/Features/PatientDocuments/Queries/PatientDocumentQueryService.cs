using BigSmile.Application.Features.PatientDocuments.Dtos;
using BigSmile.Application.Interfaces.Repositories;
using BigSmile.Application.Interfaces.Storage;
using BigSmile.SharedKernel.Context;

namespace BigSmile.Application.Features.PatientDocuments.Queries
{
    public sealed record PatientDocumentDownloadResult(
        string OriginalFileName,
        string ContentType,
        Stream ContentStream);

    public interface IPatientDocumentQueryService
    {
        Task<IReadOnlyList<PatientDocumentSummaryDto>?> ListActiveByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
        Task<PatientDocumentDownloadResult?> DownloadAsync(Guid patientId, Guid documentId, CancellationToken cancellationToken = default);
    }

    public sealed class PatientDocumentQueryService : IPatientDocumentQueryService
    {
        private readonly IPatientDocumentRepository _patientDocumentRepository;
        private readonly IPatientDocumentBinaryStore _patientDocumentBinaryStore;
        private readonly IPatientRepository _patientRepository;
        private readonly ITenantContext _tenantContext;

        public PatientDocumentQueryService(
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

        public async Task<IReadOnlyList<PatientDocumentSummaryDto>?> ListActiveByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            EnsureDocumentAccessContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            return (await _patientDocumentRepository.ListActiveByPatientIdAsync(patientId, cancellationToken))
                .OrderByDescending(document => document.UploadedAtUtc)
                .ThenByDescending(document => document.Id)
                .Select(document => document.ToSummaryDto())
                .ToList();
        }

        public async Task<PatientDocumentDownloadResult?> DownloadAsync(
            Guid patientId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            EnsureDocumentAccessContext();

            var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken);
            if (patient is null)
            {
                return null;
            }

            var patientDocument = await _patientDocumentRepository.GetActiveByIdAsync(patientId, documentId, cancellationToken);
            if (patientDocument is null)
            {
                return null;
            }

            var contentStream = await _patientDocumentBinaryStore.OpenReadAsync(patientDocument.StorageKey, cancellationToken);
            if (contentStream is null)
            {
                throw new InvalidOperationException("The requested patient document binary is not available.");
            }

            return new PatientDocumentDownloadResult(
                patientDocument.OriginalFileName,
                patientDocument.ContentType,
                contentStream);
        }

        private void EnsureDocumentAccessContext()
        {
            if (_tenantContext.HasPlatformOverride())
            {
                return;
            }

            var tenantIdValue = _tenantContext.GetTenantId();
            if (!Guid.TryParse(tenantIdValue, out var tenantId) || tenantId == Guid.Empty)
            {
                throw new InvalidOperationException("Patient document queries require a resolved tenant context or explicit platform override.");
            }
        }
    }
}
