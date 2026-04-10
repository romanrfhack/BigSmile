using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public class Patient : Entity<Guid>, ITenantOwnedEntity
    {
        private const int DefaultMaxNameLength = 100;
        private const int ClinicalAlertsSummaryMaxLength = 500;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public DateOnly DateOfBirth { get; private set; }
        public string? PrimaryPhone { get; private set; }
        public string? Email { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool HasClinicalAlerts { get; private set; }
        public string? ClinicalAlertsSummary { get; private set; }

        public string? ResponsiblePartyName { get; private set; }
        public string? ResponsiblePartyRelationship { get; private set; }
        public string? ResponsiblePartyPhone { get; private set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; private set; }

        public string FullName => $"{FirstName} {LastName}".Trim();

        protected Patient()
        {
        }

        public Patient(
            Guid tenantId,
            string firstName,
            string lastName,
            DateOnly dateOfBirth,
            string? primaryPhone = null,
            string? email = null,
            bool isActive = true,
            bool hasClinicalAlerts = false,
            string? clinicalAlertsSummary = null,
            string? responsiblePartyName = null,
            string? responsiblePartyRelationship = null,
            string? responsiblePartyPhone = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Patient tenant ownership is required.", nameof(tenantId));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            FirstName = NormalizeRequired(firstName, nameof(firstName), DefaultMaxNameLength);
            LastName = NormalizeRequired(lastName, nameof(lastName), DefaultMaxNameLength);
            DateOfBirth = EnsureValidDateOfBirth(dateOfBirth);
            PrimaryPhone = NormalizeOptional(primaryPhone, nameof(primaryPhone), 40);
            Email = NormalizeOptional(email, nameof(email), 256);
            IsActive = isActive;

            SetClinicalAlerts(hasClinicalAlerts, clinicalAlertsSummary);
            SetResponsibleParty(responsiblePartyName, responsiblePartyRelationship, responsiblePartyPhone);
        }

        public void UpdateProfile(
            string firstName,
            string lastName,
            DateOnly dateOfBirth,
            string? primaryPhone,
            string? email,
            bool isActive,
            bool hasClinicalAlerts,
            string? clinicalAlertsSummary,
            string? responsiblePartyName,
            string? responsiblePartyRelationship,
            string? responsiblePartyPhone)
        {
            FirstName = NormalizeRequired(firstName, nameof(firstName), DefaultMaxNameLength);
            LastName = NormalizeRequired(lastName, nameof(lastName), DefaultMaxNameLength);
            DateOfBirth = EnsureValidDateOfBirth(dateOfBirth);
            PrimaryPhone = NormalizeOptional(primaryPhone, nameof(primaryPhone), 40);
            Email = NormalizeOptional(email, nameof(email), 256);
            IsActive = isActive;

            SetClinicalAlerts(hasClinicalAlerts, clinicalAlertsSummary);
            SetResponsibleParty(responsiblePartyName, responsiblePartyRelationship, responsiblePartyPhone);
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive)
            {
                return;
            }

            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        private void SetResponsibleParty(
            string? responsiblePartyName,
            string? responsiblePartyRelationship,
            string? responsiblePartyPhone)
        {
            var normalizedName = NormalizeOptional(responsiblePartyName, nameof(responsiblePartyName), DefaultMaxNameLength);
            var normalizedRelationship = NormalizeOptional(responsiblePartyRelationship, nameof(responsiblePartyRelationship), 100);
            var normalizedPhone = NormalizeOptional(responsiblePartyPhone, nameof(responsiblePartyPhone), 40);

            if ((normalizedRelationship is not null || normalizedPhone is not null) && normalizedName is null)
            {
                throw new ArgumentException("Responsible party name is required when additional responsible party data is provided.");
            }

            ResponsiblePartyName = normalizedName;
            ResponsiblePartyRelationship = normalizedRelationship;
            ResponsiblePartyPhone = normalizedPhone;
        }

        private void SetClinicalAlerts(bool hasClinicalAlerts, string? clinicalAlertsSummary)
        {
            HasClinicalAlerts = hasClinicalAlerts;
            ClinicalAlertsSummary = hasClinicalAlerts
                ? NormalizeOptional(clinicalAlertsSummary, nameof(clinicalAlertsSummary), ClinicalAlertsSummaryMaxLength)
                : null;
        }

        private static DateOnly EnsureValidDateOfBirth(DateOnly dateOfBirth)
        {
            if (dateOfBirth == default)
            {
                throw new ArgumentException("Patient date of birth is required.", nameof(dateOfBirth));
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (dateOfBirth > today)
            {
                throw new ArgumentException("Patient date of birth cannot be in the future.", nameof(dateOfBirth));
            }

            return dateOfBirth;
        }

        private static string NormalizeRequired(string value, string paramName, int maxLength)
        {
            var normalized = NormalizeOptional(value, paramName, maxLength);
            if (normalized is null)
            {
                throw new ArgumentException($"{paramName} is required.", paramName);
            }

            return normalized;
        }

        private static string? NormalizeOptional(string? value, string paramName, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            if (normalized.Length > maxLength)
            {
                throw new ArgumentException($"{paramName} exceeds the allowed length of {maxLength}.", paramName);
            }

            return normalized;
        }
    }
}
