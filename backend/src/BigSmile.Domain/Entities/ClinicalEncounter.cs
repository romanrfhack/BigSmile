using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public sealed class ClinicalEncounter : Entity<Guid>, ITenantOwnedEntity
    {
        public const int ChiefComplaintMaxLength = 500;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid ClinicalRecordId { get; private set; }
        public ClinicalRecord ClinicalRecord { get; private set; } = null!;

        public Guid PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public DateTime OccurredAtUtc { get; private set; }
        public string ChiefComplaint { get; private set; } = string.Empty;
        public ClinicalEncounterConsultationType ConsultationType { get; private set; }
        public decimal? TemperatureC { get; private set; }
        public int? BloodPressureSystolic { get; private set; }
        public int? BloodPressureDiastolic { get; private set; }
        public decimal? WeightKg { get; private set; }
        public decimal? HeightCm { get; private set; }
        public int? RespiratoryRatePerMinute { get; private set; }
        public int? HeartRateBpm { get; private set; }
        public Guid? ClinicalNoteId { get; private set; }
        public ClinicalNote? ClinicalNote { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private ClinicalEncounter()
        {
        }

        internal ClinicalEncounter(
            Guid tenantId,
            Guid clinicalRecordId,
            Guid patientId,
            DateTime occurredAtUtc,
            string chiefComplaint,
            ClinicalEncounterConsultationType consultationType,
            decimal? temperatureC,
            int? bloodPressureSystolic,
            int? bloodPressureDiastolic,
            decimal? weightKg,
            decimal? heightCm,
            int? respiratoryRatePerMinute,
            int? heartRateBpm,
            ClinicalNote? clinicalNote,
            Guid createdByUserId)
        {
            EnsureReference(tenantId, "Clinical encounter tenant ownership is required.", nameof(tenantId));
            EnsureReference(clinicalRecordId, "Clinical record reference is required.", nameof(clinicalRecordId));
            EnsureReference(patientId, "Clinical encounter patient reference is required.", nameof(patientId));
            EnsureActor(createdByUserId);

            if (clinicalNote?.Id == Guid.Empty)
            {
                throw new ArgumentException("Clinical note reference cannot be empty.", nameof(clinicalNote));
            }

            if (clinicalNote is not null && clinicalNote.ClinicalRecordId != clinicalRecordId)
            {
                throw new InvalidOperationException("Clinical encounter note must belong to the same clinical record.");
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            ClinicalRecordId = clinicalRecordId;
            PatientId = patientId;
            OccurredAtUtc = EnsureOccurredAt(occurredAtUtc);
            ChiefComplaint = NormalizeRequired(chiefComplaint, nameof(chiefComplaint), ChiefComplaintMaxLength);
            ConsultationType = EnsureDefinedConsultationType(consultationType);
            TemperatureC = EnsureRange(temperatureC, 30.0m, 45.0m, nameof(temperatureC));
            BloodPressureSystolic = EnsureRange(bloodPressureSystolic, 50, 260, nameof(bloodPressureSystolic));
            BloodPressureDiastolic = EnsureRange(bloodPressureDiastolic, 30, 180, nameof(bloodPressureDiastolic));
            EnsureBloodPressurePair(BloodPressureSystolic, BloodPressureDiastolic);
            WeightKg = EnsureRange(weightKg, 0.5m, 500.0m, nameof(weightKg));
            HeightCm = EnsureRange(heightCm, 30.0m, 250.0m, nameof(heightCm));
            RespiratoryRatePerMinute = EnsureRange(respiratoryRatePerMinute, 5, 80, nameof(respiratoryRatePerMinute));
            HeartRateBpm = EnsureRange(heartRateBpm, 20, 240, nameof(heartRateBpm));
            ClinicalNoteId = clinicalNote?.Id;
            ClinicalNote = clinicalNote;
            CreatedByUserId = createdByUserId;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void LinkClinicalNote(ClinicalNote clinicalNote)
        {
            if (clinicalNote is null)
            {
                throw new ArgumentNullException(nameof(clinicalNote));
            }

            if (clinicalNote.Id == Guid.Empty)
            {
                throw new ArgumentException("Clinical note reference cannot be empty.", nameof(clinicalNote));
            }

            if (clinicalNote.ClinicalRecordId != ClinicalRecordId)
            {
                throw new InvalidOperationException("Clinical encounter note must belong to the same clinical record.");
            }

            ClinicalNoteId = clinicalNote.Id;
            ClinicalNote = clinicalNote;
        }

        private static DateTime EnsureOccurredAt(DateTime occurredAtUtc)
        {
            if (occurredAtUtc == default)
            {
                throw new ArgumentException("Clinical encounter occurrence date/time is required.", nameof(occurredAtUtc));
            }

            return occurredAtUtc;
        }

        private static ClinicalEncounterConsultationType EnsureDefinedConsultationType(
            ClinicalEncounterConsultationType consultationType)
        {
            if (!Enum.IsDefined(typeof(ClinicalEncounterConsultationType), consultationType))
            {
                throw new ArgumentException("Clinical encounter consultation type is not supported.", nameof(consultationType));
            }

            return consultationType;
        }

        private static void EnsureBloodPressurePair(int? systolic, int? diastolic)
        {
            if (systolic.HasValue != diastolic.HasValue)
            {
                throw new ArgumentException("Blood pressure requires both systolic and diastolic values.");
            }

            if (systolic.HasValue && diastolic.HasValue && diastolic.Value >= systolic.Value)
            {
                throw new ArgumentException("Blood pressure diastolic value must be lower than systolic value.");
            }
        }

        private static decimal? EnsureRange(decimal? value, decimal minimum, decimal maximum, string paramName)
        {
            if (!value.HasValue)
            {
                return null;
            }

            if (value.Value < minimum || value.Value > maximum)
            {
                throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be between {minimum} and {maximum}.");
            }

            return value.Value;
        }

        private static int? EnsureRange(int? value, int minimum, int maximum, string paramName)
        {
            if (!value.HasValue)
            {
                return null;
            }

            if (value.Value < minimum || value.Value > maximum)
            {
                throw new ArgumentOutOfRangeException(paramName, $"{paramName} must be between {minimum} and {maximum}.");
            }

            return value.Value;
        }

        private static void EnsureReference(Guid value, string message, string paramName)
        {
            if (value == Guid.Empty)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        private static void EnsureActor(Guid createdByUserId)
        {
            if (createdByUserId == Guid.Empty)
            {
                throw new ArgumentException("Clinical encounter author is required.", nameof(createdByUserId));
            }
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
