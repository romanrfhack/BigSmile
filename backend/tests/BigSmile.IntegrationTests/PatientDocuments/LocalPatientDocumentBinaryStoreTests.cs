using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BigSmile.IntegrationTests.PatientDocuments
{
    public sealed class LocalPatientDocumentBinaryStoreTests
    {
        [Fact]
        public async Task StorageOperations_RejectKeysResolvingOutsideConfiguredRoot()
        {
            var parentPath = Path.Combine(
                Path.GetTempPath(),
                "bigsmile-patient-document-containment-tests",
                Guid.NewGuid().ToString("N"));
            var rootPath = Path.Combine(parentPath, "root");
            var outsidePath = Path.Combine(parentPath, "outside.pdf");
            Directory.CreateDirectory(rootPath);

            try
            {
                var store = CreateStore(rootPath);
                using var content = new MemoryStream("%PDF-1.7\ncontent"u8.ToArray());

                await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    store.SaveAsync("../outside.pdf", content));

                Assert.Throws<InvalidOperationException>(() =>
                    store.OpenReadAsync("../outside.pdf"));

                Assert.Throws<InvalidOperationException>(() =>
                    store.DeleteIfExistsAsync("../outside.pdf"));

                Assert.False(File.Exists(outsidePath));
            }
            finally
            {
                if (Directory.Exists(parentPath))
                {
                    Directory.Delete(parentPath, recursive: true);
                }
            }
        }

        [Fact]
        public async Task StorageOperations_SucceedForKeyInsideConfiguredRoot()
        {
            var rootPath = Path.Combine(
                Path.GetTempPath(),
                "bigsmile-patient-document-containment-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(rootPath);

            try
            {
                var store = CreateStore(rootPath);
                const string storageKey = "patients/tenant/patient/document.pdf";
                var expectedContent = "%PDF-1.7\ncontent"u8.ToArray();
                using var content = new MemoryStream(expectedContent);

                await store.SaveAsync(storageKey, content);

                await using var storedContent = await store.OpenReadAsync(storageKey);
                Assert.NotNull(storedContent);
                using var buffer = new MemoryStream();
                await storedContent!.CopyToAsync(buffer);
                Assert.Equal(expectedContent, buffer.ToArray());

                await store.DeleteIfExistsAsync(storageKey);
                Assert.Null(await store.OpenReadAsync(storageKey));
            }
            finally
            {
                if (Directory.Exists(rootPath))
                {
                    Directory.Delete(rootPath, recursive: true);
                }
            }
        }

        private static LocalPatientDocumentBinaryStore CreateStore(string rootPath)
        {
            return new LocalPatientDocumentBinaryStore(
                Options.Create(new PatientDocumentStorageOptions
                {
                    RootPath = rootPath
                }),
                new TestHostEnvironment(rootPath));
        }

        private sealed class TestHostEnvironment : IHostEnvironment
        {
            public TestHostEnvironment(string contentRootPath)
            {
                ContentRootPath = contentRootPath;
                ApplicationName = "BigSmile.IntegrationTests";
                EnvironmentName = Environments.Development;
                ContentRootFileProvider = new PhysicalFileProvider(contentRootPath);
            }

            public string EnvironmentName { get; set; }

            public string ApplicationName { get; set; }

            public string ContentRootPath { get; set; }

            public IFileProvider ContentRootFileProvider { get; set; }
        }
    }
}
