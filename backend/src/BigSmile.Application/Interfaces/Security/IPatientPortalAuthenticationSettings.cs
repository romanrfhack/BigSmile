namespace BigSmile.Application.Interfaces.Security
{
    public interface IPatientPortalAuthenticationSettings
    {
        int PasswordHashIterationCount { get; }
        int MinimumPasswordLength { get; }
        int MaximumPasswordLength { get; }
        int MaximumFailedLoginAttempts { get; }
        TimeSpan LockoutDuration { get; }
    }
}
