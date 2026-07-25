namespace BigSmile.Application.Interfaces.PatientIntakes
{
    public interface IPatientIntakeDraftSettings
    {
        TimeSpan DraftLifetime { get; }
    }
}
