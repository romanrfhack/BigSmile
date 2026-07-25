namespace BigSmile.Application.Interfaces.PatientIntakes
{
    public interface IPatientIntakeAccessLinkSettings
    {
        TimeSpan WaitingRoomLinkLifetime { get; }
    }
}
