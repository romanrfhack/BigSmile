using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientPortalInvitations.Dtos
{
    public sealed record PatientPortalInvitationSummaryDto(
        Guid Id,
        Guid PatientId,
        string Purpose,
        string Status,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc,
        DateTime? RevokedAtUtc,
        DateTime? ConsumedAtUtc);

    public sealed record IssuedPatientPortalInvitationDto(
        Guid Id,
        Guid PatientId,
        string Purpose,
        string ActivationToken,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc);

    internal static class PatientPortalInvitationDtoMappings
    {
        public static PatientPortalInvitationSummaryDto ToSummaryDto(
            this PatientPortalInvitation invitation,
            DateTime utcNow)
        {
            ArgumentNullException.ThrowIfNull(invitation);

            var status = invitation.ConsumedAtUtc.HasValue
                ? "Consumed"
                : invitation.RevokedAtUtc.HasValue
                    ? "Revoked"
                    : invitation.IsExpiredAt(utcNow)
                        ? "Expired"
                        : invitation.CanBeConsumedAt(utcNow)
                            ? "Active"
                            : "Pending";

            return new PatientPortalInvitationSummaryDto(
                invitation.Id,
                invitation.PatientId,
                invitation.Purpose.ToString(),
                status,
                invitation.CreatedAtUtc,
                invitation.ExpiresAtUtc,
                invitation.RevokedAtUtc,
                invitation.ConsumedAtUtc);
        }
    }
}
