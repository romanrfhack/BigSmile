using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class PatientDocument : Entity<Guid>, ITenantOwnedEntity
    {
        public const long MaxFileSizeBytes = 10 * 1024 * 1024;
        public const int OriginalFileNameMaxLength = 255;
        public const int ContentTypeMaxLength = 100;
        public const int StorageKeyMaxLength = 500;

        private static readonly HashSet<string> AllowedContentTypesSet = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png"
        };

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public string OriginalFileName { get; private set; } = string.Empty;
        public string ContentType { get; private set; } = string.Empty;
        public long SizeBytes { get; private set; }
        public string StorageKey { get; private set; } = string.Empty;
        public DateTime UploadedAtUtc { get; private set; }
        public Guid UploadedByUserId { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }
        public Guid? DeletedByUserId { get; private set; }

        private PatientDocument()
        {
        }

        public PatientDocument(
            Guid tenantId,
            Guid patientId,
            string originalFileName,
            string contentType,
            long sizeBytes,
            string storageKey,
            Guid uploadedByUserId)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Patient document tenant ownership is required.", nameof(tenantId));
            }

            if (patientId == Guid.Empty)
            {
                throw new ArgumentException("Patient document patient reference is required.", nameof(patientId));
            }

            EnsureActor(uploadedByUserId, nameof(uploadedByUserId));

            Id = Guid.NewGuid();
            TenantId = tenantId;
            PatientId = patientId;
            OriginalFileName = NormalizeRequired(originalFileName, nameof(originalFileName), OriginalFileNameMaxLength);
            ContentType = NormalizeContentType(contentType);
            SizeBytes = NormalizeSizeBytes(sizeBytes);
            StorageKey = NormalizeRequired(storageKey, nameof(storageKey), StorageKeyMaxLength);
            UploadedAtUtc = DateTime.UtcNow;
            UploadedByUserId = uploadedByUserId;
        }

        public bool Retire(Guid deletedByUserId)
        {
            EnsureActor(deletedByUserId, nameof(deletedByUserId));

            if (DeletedAtUtc.HasValue)
            {
                throw new InvalidOperationException("Patient document is already retired.");
            }

            DeletedAtUtc = DateTime.UtcNow;
            DeletedByUserId = deletedByUserId;
            return true;
        }

        public static bool IsAllowedContentType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            return AllowedContentTypesSet.Contains(contentType.Trim());
        }

        private static string NormalizeContentType(string? contentType)
        {
            var normalized = NormalizeRequired(contentType, nameof(contentType), ContentTypeMaxLength).ToLowerInvariant();
            if (!AllowedContentTypesSet.Contains(normalized))
            {
                throw new ArgumentException(
                    "Patient document content type must be one of: application/pdf, image/jpeg, image/png.",
                    nameof(contentType));
            }

            return normalized;
        }

        private static long NormalizeSizeBytes(long sizeBytes)
        {
            if (sizeBytes <= 0)
            {
                throw new ArgumentException("Patient document size must be greater than zero.", nameof(sizeBytes));
            }

            if (sizeBytes > MaxFileSizeBytes)
            {
                throw new ArgumentException(
                    $"Patient document size must not exceed {MaxFileSizeBytes} bytes.",
                    nameof(sizeBytes));
            }

            return sizeBytes;
        }

        private static string NormalizeRequired(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"{paramName} is required.", paramName);
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} exceeds the allowed length of {maxLength}.", paramName);
            }

            return normalized;
        }

        private static void EnsureActor(Guid actorUserId, string paramName)
        {
            if (actorUserId == Guid.Empty)
            {
                throw new ArgumentException("Patient document actor is required.", paramName);
            }
        }
    }
}
