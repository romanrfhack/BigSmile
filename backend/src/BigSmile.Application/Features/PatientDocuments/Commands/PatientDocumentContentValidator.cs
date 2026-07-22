namespace BigSmile.Application.Features.PatientDocuments.Commands
{
    internal static class PatientDocumentContentValidator
    {
        private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
        private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
        private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        private const int MaxSignatureLength = 8;

        public static async Task ValidateAsync(
            string contentType,
            long declaredSizeBytes,
            Stream contentStream,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contentStream);

            if (!contentStream.CanRead)
            {
                throw new ArgumentException("Patient document content stream must be readable.", nameof(contentStream));
            }

            if (!contentStream.CanSeek)
            {
                throw new ArgumentException(
                    "Patient document content stream must support seeking so its signature can be validated safely.",
                    nameof(contentStream));
            }

            if (contentStream.Length != declaredSizeBytes)
            {
                throw new ArgumentException(
                    "Patient document declared size does not match the uploaded content length.",
                    nameof(declaredSizeBytes));
            }

            var normalizedContentType = contentType.Trim().ToLowerInvariant();
            var expectedSignature = normalizedContentType switch
            {
                "application/pdf" => PdfSignature,
                "image/jpeg" => JpegSignature,
                "image/png" => PngSignature,
                _ => throw new ArgumentException(
                    "Patient document content type must be one of: application/pdf, image/jpeg, image/png.",
                    nameof(contentType))
            };

            contentStream.Seek(0, SeekOrigin.Begin);

            try
            {
                var header = new byte[MaxSignatureLength];
                var bytesRead = 0;

                while (bytesRead < expectedSignature.Length)
                {
                    var read = await contentStream.ReadAsync(
                        header.AsMemory(bytesRead, expectedSignature.Length - bytesRead),
                        cancellationToken);

                    if (read == 0)
                    {
                        break;
                    }

                    bytesRead += read;
                }

                if (bytesRead < expectedSignature.Length ||
                    !header.AsSpan(0, expectedSignature.Length).SequenceEqual(expectedSignature))
                {
                    throw new ArgumentException(
                        $"Patient document content does not match declared content type '{normalizedContentType}'.",
                        nameof(contentStream));
                }
            }
            finally
            {
                contentStream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
