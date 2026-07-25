using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos
{
    public sealed record IssuedPatientIntakeAccessLinkDto(
        Guid AccessLinkId,
        Guid? BranchId,
        string? BranchName,
        string BootstrapToken,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc);

    public sealed record PatientIntakeAccessLinkSummaryDto(
        Guid AccessLinkId,
        Guid? BranchId,
        string? BranchName,
        string Status,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc,
        DateTime? RevokedAtUtc,
        DateTime? ConsumedAtUtc,
        bool CanRevoke);

    internal static class PatientIntakeAccessLinkDtoMappings
    {
        internal static PatientIntakeAccessLinkSummaryDto ToSummaryDto(
            this PatientIntakeAccessLink accessLink,
            DateTime utcNow)
        {
            ArgumentNullException.ThrowIfNull(accessLink);

            var status = accessLink.ConsumedAtUtc.HasValue
                ? "Consumed"
                : accessLink.RevokedAtUtc.HasValue
                    ? "Revoked"
                    : accessLink.IsExpiredAt(utcNow)
                        ? "Expired"
                        : "Active";

            return new PatientIntakeAccessLinkSummaryDto(
                accessLink.Id,
                accessLink.BranchId,
                accessLink.Branch?.Name,
                status,
                accessLink.CreatedAtUtc,
                accessLink.ExpiresAtUtc,
                accessLink.RevokedAtUtc,
                accessLink.ConsumedAtUtc,
                string.Equals(status, "Active", StringComparison.Ordinal));
        }
    }
}
