namespace BigSmile.Domain.Entities
{
    public enum PatientIntakeAuthenticationAuditAction
    {
        AccountActivated = 1,
        LinkConsumed = 2,
        LoginSucceeded = 3,
        LoginFailed = 4,
        AccountLocked = 5,
        SessionsRevoked = 6
    }
}
