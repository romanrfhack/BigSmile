using BigSmile.SharedKernel;
using BigSmile.SharedKernel.Multitenancy;

namespace BigSmile.Domain.Entities
{
    public class AppointmentBlock : Entity<Guid>, ITenantOwnedEntity
    {
        private const int LabelMaxLength = 200;

        public Guid TenantId { get; private set; }
        public Tenant Tenant { get; private set; } = null!;

        public Guid BranchId { get; private set; }
        public Branch Branch { get; private set; } = null!;

        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }
        public string? Label { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        protected AppointmentBlock()
        {
        }

        public AppointmentBlock(
            Guid tenantId,
            Guid branchId,
            DateTime startsAt,
            DateTime endsAt,
            string? label = null)
        {
            if (tenantId == Guid.Empty)
            {
                throw new ArgumentException("Appointment block tenant ownership is required.", nameof(tenantId));
            }

            if (branchId == Guid.Empty)
            {
                throw new ArgumentException("Appointment block branch is required.", nameof(branchId));
            }

            Id = Guid.NewGuid();
            TenantId = tenantId;
            BranchId = branchId;
            SetSchedule(startsAt, endsAt);
            Label = NormalizeOptional(label, nameof(label), LabelMaxLength);
        }

        public bool Overlaps(DateTime startsAt, DateTime endsAt)
        {
            ValidateRange(startsAt, endsAt);
            return StartsAt < endsAt && EndsAt > startsAt;
        }

        private void SetSchedule(DateTime startsAt, DateTime endsAt)
        {
            ValidateRange(startsAt, endsAt);

            StartsAt = startsAt;
            EndsAt = endsAt;
        }

        private static void ValidateRange(DateTime startsAt, DateTime endsAt)
        {
            if (startsAt == default)
            {
                throw new ArgumentException("Appointment block start time is required.", nameof(startsAt));
            }

            if (endsAt == default)
            {
                throw new ArgumentException("Appointment block end time is required.", nameof(endsAt));
            }

            if (endsAt <= startsAt)
            {
                throw new ArgumentException("Appointment block end time must be after the start time.", nameof(endsAt));
            }
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
