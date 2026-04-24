namespace BigSmile.Application.Features.PatientDocuments.Dtos
{
    public sealed record PatientDocumentSummaryDto(
        Guid DocumentId,
        Guid PatientId,
        string OriginalFileName,
        string ContentType,
        long SizeBytes,
        DateTime UploadedAtUtc,
        Guid UploadedByUserId);
}
