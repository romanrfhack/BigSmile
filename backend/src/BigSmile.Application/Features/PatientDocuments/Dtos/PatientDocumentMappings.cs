using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientDocuments.Dtos
{
    internal static class PatientDocumentMappings
    {
        public static PatientDocumentSummaryDto ToSummaryDto(this PatientDocument patientDocument)
        {
            return new PatientDocumentSummaryDto(
                patientDocument.Id,
                patientDocument.PatientId,
                patientDocument.OriginalFileName,
                patientDocument.ContentType,
                patientDocument.SizeBytes,
                patientDocument.UploadedAtUtc,
                patientDocument.UploadedByUserId);
        }
    }
}
