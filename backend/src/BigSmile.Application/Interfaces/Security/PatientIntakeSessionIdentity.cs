namespace BigSmile.Application.Interfaces.Security
{
    public sealed record PatientIntakeSessionIdentity(
        Guid AccountId,
        Guid TenantId,
        Guid IntakeId,
        int SessionVersion);

    public interface IPatientIntakeSessionValidator
    {
        Task<bool> ValidateAsync(
            PatientIntakeSessionIdentity identity,
            CancellationToken cancellationToken = default);
    }
}
