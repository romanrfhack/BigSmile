namespace BigSmile.Application.Interfaces.Security
{
    public sealed record PatientPortalSessionIdentity(
        Guid AccountId,
        Guid TenantId,
        Guid PatientId,
        int SessionVersion);

    public interface IPatientPortalSessionValidator
    {
        Task<bool> ValidateAsync(
            PatientPortalSessionIdentity identity,
            CancellationToken cancellationToken = default);
    }
}
