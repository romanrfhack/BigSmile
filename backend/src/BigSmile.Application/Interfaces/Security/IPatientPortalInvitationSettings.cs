namespace BigSmile.Application.Interfaces.Security
{
    public interface IPatientPortalInvitationSettings
    {
        TimeSpan ExistingPatientActivationLifetime { get; }
    }
}
