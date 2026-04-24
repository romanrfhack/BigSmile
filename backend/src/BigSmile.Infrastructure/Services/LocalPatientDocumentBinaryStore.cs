using BigSmile.Application.Interfaces.Storage;
using BigSmile.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BigSmile.Infrastructure.Services
{
    public sealed class LocalPatientDocumentBinaryStore : IPatientDocumentBinaryStore
    {
        private readonly string _rootPath;

        public LocalPatientDocumentBinaryStore(
            IOptions<PatientDocumentStorageOptions> options,
            IHostEnvironment hostEnvironment)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(hostEnvironment);

            var configuredRootPath = options.Value.RootPath;
            _rootPath = ResolveRootPath(configuredRootPath, hostEnvironment.ContentRootPath);
        }

        public async Task SaveAsync(string storageKey, Stream content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);

            var fullPath = ResolveStoragePath(storageKey);
            var directoryPath = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            await using var destinationStream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true);

            if (content.CanSeek)
            {
                content.Seek(0, SeekOrigin.Begin);
            }

            await content.CopyToAsync(destinationStream, cancellationToken);
        }

        public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            var fullPath = ResolveStoragePath(storageKey);
            if (!File.Exists(fullPath))
            {
                return Task.FromResult<Stream?>(null);
            }

            Stream stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                useAsync: true);

            return Task.FromResult<Stream?>(stream);
        }

        public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            var fullPath = ResolveStoragePath(storageKey);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        private string ResolveStoragePath(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
            {
                throw new ArgumentException("Patient document storage key is required.", nameof(storageKey));
            }

            var relativePath = storageKey
                .Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
            var fullRootPath = Path.GetFullPath(_rootPath);
            var rootPrefix = fullRootPath.EndsWith(Path.DirectorySeparatorChar)
                ? fullRootPath
                : $"{fullRootPath}{Path.DirectorySeparatorChar}";

            if (!string.Equals(fullPath, fullRootPath, StringComparison.OrdinalIgnoreCase) &&
                !fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Patient document storage key resolved outside the configured root path.");
            }

            return fullPath;
        }

        private static string ResolveRootPath(string? configuredRootPath, string contentRootPath)
        {
            if (string.IsNullOrWhiteSpace(configuredRootPath))
            {
                return Path.Combine(contentRootPath, "App_Data", "PatientDocuments");
            }

            return Path.IsPathRooted(configuredRootPath)
                ? configuredRootPath
                : Path.Combine(contentRootPath, configuredRootPath);
        }
    }
}
