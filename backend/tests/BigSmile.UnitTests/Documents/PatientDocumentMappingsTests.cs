using BigSmile.Application.Features.PatientDocuments.Dtos;
using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Documents
{
    public class PatientDocumentMappingsTests
    {
        [Fact]
        public void ToSummaryDto_ExposesPublicMetadataWithoutStorageKey()
        {
            var patientDocument = new PatientDocument(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "intake-form.pdf",
                "application/pdf",
                2048,
                "patients/tenant/patient/intake-form.pdf",
                Guid.NewGuid());

            var dto = patientDocument.ToSummaryDto();

            Assert.Equal(patientDocument.Id, dto.DocumentId);
            Assert.Equal(patientDocument.PatientId, dto.PatientId);
            Assert.Equal("intake-form.pdf", dto.OriginalFileName);
            Assert.Equal("application/pdf", dto.ContentType);
            Assert.Equal(2048, dto.SizeBytes);
            Assert.Equal(patientDocument.UploadedByUserId, dto.UploadedByUserId);
            Assert.Null(typeof(PatientDocumentSummaryDto).GetProperty(nameof(PatientDocument.StorageKey)));
        }
    }
}
