namespace BigSmile.Application.Interfaces.Storage
{
    public interface IPatientDocumentBinaryStore
    {
        Task SaveAsync(string storageKey, Stream content, CancellationToken cancellationToken = default);
        Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default);
        Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default);
    }
}
