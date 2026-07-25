namespace BigSmile.Domain.Entities
{
    public enum PatientPortalAuthenticationAuditAction
    {
        AccountActivated = 1,
        AccountRecovered = 2,
        InvitationConsumed = 3,
        LoginSucceeded = 4,
        LoginFailed = 5,
        AccountLocked = 6,
        SessionsRevoked = 7,
        RecoveryStarted = 8
    }
}
