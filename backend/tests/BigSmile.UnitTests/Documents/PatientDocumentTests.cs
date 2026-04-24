using BigSmile.Domain.Entities;

namespace BigSmile.UnitTests.Documents
{
    public class PatientDocumentTests
    {
        [Fact]
        public void Constructor_CapturesMinimalMetadataForAllowedUpload()
        {
            var tenantId = Guid.NewGuid();
            var patientId = Guid.NewGuid();
            var actorUserId = Guid.NewGuid();

            var patientDocument = new PatientDocument(
                tenantId,
                patientId,
                "  radiography.pdf  ",
                "Application/Pdf",
                1024,
                "patients/tenant/patient/radiography.pdf",
                actorUserId);

            Assert.Equal(tenantId, patientDocument.TenantId);
            Assert.Equal(patientId, patientDocument.PatientId);
            Assert.Equal("radiography.pdf", patientDocument.OriginalFileName);
            Assert.Equal("application/pdf", patientDocument.ContentType);
            Assert.Equal(1024, patientDocument.SizeBytes);
            Assert.Equal(actorUserId, patientDocument.UploadedByUserId);
            Assert.Null(patientDocument.DeletedAtUtc);
            Assert.Null(patientDocument.DeletedByUserId);
        }

        [Fact]
        public void Constructor_RejectsContentTypesOutsideTheAllowlist()
        {
            var exception = Assert.Throws<ArgumentException>(() => new PatientDocument(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "voice-note.mp3",
                "audio/mpeg",
                512,
                "patients/tenant/patient/voice-note.mp3",
                Guid.NewGuid()));

            Assert.Contains("content type must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Constructor_RejectsFilesLargerThanTheSliceLimit()
        {
            var exception = Assert.Throws<ArgumentException>(() => new PatientDocument(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "large-file.pdf",
                "application/pdf",
                PatientDocument.MaxFileSizeBytes + 1,
                "patients/tenant/patient/large-file.pdf",
                Guid.NewGuid()));

            Assert.Contains("must not exceed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Retire_AddsDeleteMetadataWithoutChangingOwnership()
        {
            var patientDocument = new PatientDocument(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "scan.png",
                "image/png",
                4096,
                "patients/tenant/patient/scan.png",
                Guid.NewGuid());

            var originalTenantId = patientDocument.TenantId;
            var originalPatientId = patientDocument.PatientId;
            var deletedByUserId = Guid.NewGuid();

            var retired = patientDocument.Retire(deletedByUserId);

            Assert.True(retired);
            Assert.Equal(originalTenantId, patientDocument.TenantId);
            Assert.Equal(originalPatientId, patientDocument.PatientId);
            Assert.NotNull(patientDocument.DeletedAtUtc);
            Assert.Equal(deletedByUserId, patientDocument.DeletedByUserId);
        }
    }
}
