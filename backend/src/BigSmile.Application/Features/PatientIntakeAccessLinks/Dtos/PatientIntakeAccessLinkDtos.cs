using BigSmile.Domain.Entities;

namespace BigSmile.Application.Features.PatientIntakeAccessLinks.Dtos
{
    public sealed record PatientIntakeAccessLinkSummaryDto(
        Guid Id,
        Guid? BranchId,
        string Purpose,
        string Status,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc,
        DateTime? RevokedAtUtc,
        DateTime? ConsumedAtUtc);

    public sealed record IssuedPatientIntakeAccessLinkDto(
        Guid Id,
        Guid? BranchId,
        string Purpose,
        string AccessToken,
        DateTime CreatedAtUtc,
        DateTime ExpiresAtUtc);

    public enum PatientIntakeAccessLinkIssueFailure
    {
        None = 0,
        BranchUnavailable = 1,
        ConcurrentConflict = 2
    }

    public sealed record PatientIntakeAccessLinkIssueResult(
        IssuedPatientIntakeAccessLinkDto? Link,
        PatientIntakeAccessLinkIssueFailure Failure)
    {
        public bool Succeeded => Link is not null && Failure == PatientIntakeAccessLinkIssueFailure.None;

        public static PatientIntakeAccessLinkIssueResult Success(
            IssuedPatientIntakeAccessLinkDto link)
        {
            ArgumentNullException.ThrowIfNull(link);
            return new PatientIntakeAccessLinkIssueResult(
                link,
                PatientIntakeAccessLinkIssueFailure.None);
        }

        public static PatientIntakeAccessLinkIssueResult Failed(
            PatientIntakeAccessLinkIssueFailure failure)
        {
            if (failure == PatientIntakeAccessLinkIssueFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeAccessLinkIssueResult(null, failure);
        }
    }

    public enum PatientIntakeAccessLinkRevokeFailure
    {
        None = 0,
        Missing = 1,
        NotActive = 2,
        ConcurrentConflict = 3
    }

    public sealed record PatientIntakeAccessLinkRevokeResult(
        PatientIntakeAccessLinkRevokeFailure Failure)
    {
        public bool Succeeded => Failure == PatientIntakeAccessLinkRevokeFailure.None;

        public static PatientIntakeAccessLinkRevokeResult Success()
        {
            return new PatientIntakeAccessLinkRevokeResult(
                PatientIntakeAccessLinkRevokeFailure.None);
        }

        public static PatientIntakeAccessLinkRevokeResult Failed(
            PatientIntakeAccessLinkRevokeFailure failure)
        {
            if (failure == PatientIntakeAccessLinkRevokeFailure.None)
            {
                throw new ArgumentOutOfRangeException(nameof(failure));
            }

            return new PatientIntakeAccessLinkRevokeResult(failure);
        }
    }

    internal static class PatientIntakeAccessLinkDtoMappings
    {
        public static PatientIntakeAccessLinkSummaryDto ToSummaryDto(
            this PatientIntakeAccessLink link,
            DateTime utcNow)
        {
            ArgumentNullException.ThrowIfNull(link);

            var status = link.ConsumedAtUtc.HasValue
                ? "Consumed"
                : link.RevokedAtUtc.HasValue
                    ? "Revoked"
                    : link.IsExpiredAt(utcNow)
                        ? "Expired"
                        : link.CanBeConsumedAt(utcNow)
                            ? "Active"
                            : "Pending";

            return new PatientIntakeAccessLinkSummaryDto(
                link.Id,
                link.BranchId,
                link.Purpose.ToString(),
                status,
                link.CreatedAtUtc,
                link.ExpiresAtUtc,
                link.RevokedAtUtc,
                link.ConsumedAtUtc);
        }
    }
}
