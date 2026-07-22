using BigSmile.Application.Features.PatientDocuments.Commands;

namespace BigSmile.UnitTests.Documents
{
    public sealed class PatientDocumentContentValidatorTests
    {
        public static IEnumerable<object[]> ValidDocuments()
        {
            yield return new object[]
            {
                "application/pdf",
                new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37, 0x0A }
            };
            yield return new object[]
            {
                "image/jpeg",
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 }
            };
            yield return new object[]
            {
                "image/png",
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00 }
            };
        }

        [Theory]
        [MemberData(nameof(ValidDocuments))]
        public async Task ValidateAsync_AcceptsMatchingSignatureAndRewindsStream(
            string contentType,
            byte[] content)
        {
            using var stream = new MemoryStream(content);
            stream.Position = Math.Min(2, stream.Length);

            await PatientDocumentContentValidator.ValidateAsync(
                contentType,
                content.LongLength,
                stream);

            Assert.Equal(0, stream.Position);
        }

        [Fact]
        public async Task ValidateAsync_RejectsContentThatDoesNotMatchDeclaredType()
        {
            var pngContent = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            using var stream = new MemoryStream(pngContent);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                PatientDocumentContentValidator.ValidateAsync(
                    "application/pdf",
                    pngContent.LongLength,
                    stream));

            Assert.Contains("does not match declared content type", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, stream.Position);
        }

        [Fact]
        public async Task ValidateAsync_RejectsTruncatedContent()
        {
            var truncatedPng = new byte[] { 0x89, 0x50, 0x4E };
            using var stream = new MemoryStream(truncatedPng);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                PatientDocumentContentValidator.ValidateAsync(
                    "image/png",
                    truncatedPng.LongLength,
                    stream));

            Assert.Contains("does not match declared content type", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, stream.Position);
        }

        [Fact]
        public async Task ValidateAsync_RejectsDeclaredSizeThatDoesNotMatchStream()
        {
            var pdfContent = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31 };
            using var stream = new MemoryStream(pdfContent);

            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                PatientDocumentContentValidator.ValidateAsync(
                    "application/pdf",
                    pdfContent.LongLength + 1,
                    stream));

            Assert.Contains("declared size", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
