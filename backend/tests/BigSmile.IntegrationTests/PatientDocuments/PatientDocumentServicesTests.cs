using System.Text;
using BigSmile.Application.Features.PatientDocuments.Commands;
using BigSmile.Application.Features.PatientDocuments.Queries;
using BigSmile.Application.Interfaces.Storage;
using BigSmile.Domain.Entities;
using BigSmile.Infrastructure.Context;
using BigSmile.Infrastructure.Data;
using BigSmile.Infrastructure.Data.Repositories;
using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using BigSmile.SharedKernel.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BigSmile.IntegrationTests.PatientDocuments
{
    public class PatientDocumentServicesTests
    {
        [Fact]
        public async Task UploadListDownloadAndRetire_SucceedWithinTenantScope()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var actorUserId = Guid.NewGuid();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            try
            {
                await using var context = CreateContext(databaseName, tenantContext);
                var commandService = CreateCommandService(context, tenantContext, storageRootPath);
                var queryService = CreateQueryService(context, tenantContext, storageRootPath);
                var fileBytes = Encoding.UTF8.GetBytes("pdf-binary");

                using var uploadStream = new MemoryStream(fileBytes);
                var created = await commandService.UploadAsync(
                    patient.Id,
                    new UploadPatientDocumentCommand(
                        "radiography.pdf",
                        "application/pdf",
                        fileBytes.LongLength,
                        uploadStream));

                Assert.Equal(patient.Id, created.PatientId);
                Assert.Equal("radiography.pdf", created.OriginalFileName);
                Assert.Equal("application/pdf", created.ContentType);
                Assert.Equal(fileBytes.LongLength, created.SizeBytes);
                Assert.Equal(actorUserId, created.UploadedByUserId);

                var listed = await queryService.ListActiveByPatientIdAsync(patient.Id);

                var listedDocument = Assert.Single(listed!);
                Assert.Equal(created.DocumentId, listedDocument.DocumentId);

                var downloaded = await queryService.DownloadAsync(patient.Id, created.DocumentId);

                Assert.NotNull(downloaded);
                Assert.Equal("radiography.pdf", downloaded!.OriginalFileName);
                Assert.Equal("application/pdf", downloaded.ContentType);
                await using var downloadedContentStream = downloaded.ContentStream;
                using var downloadBuffer = new MemoryStream();
                await downloadedContentStream.CopyToAsync(downloadBuffer);
                Assert.Equal(fileBytes, downloadBuffer.ToArray());

                var retired = await commandService.RetireAsync(patient.Id, created.DocumentId);

                Assert.True(retired);
                Assert.Empty((await queryService.ListActiveByPatientIdAsync(patient.Id))!);
                Assert.Null(await queryService.DownloadAsync(patient.Id, created.DocumentId));
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        [Fact]
        public async Task UploadAsync_Fails_WhenContentTypeIsOutsideTheAllowlist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            try
            {
                await using var context = CreateContext(databaseName, tenantContext);
                var commandService = CreateCommandService(context, tenantContext, storageRootPath);
                var bytes = Encoding.UTF8.GetBytes("unsupported");
                using var stream = new MemoryStream(bytes);

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.UploadAsync(
                    patient.Id,
                    new UploadPatientDocumentCommand(
                        "voice-note.mp3",
                        "audio/mpeg",
                        bytes.LongLength,
                        stream)));

                Assert.Contains("content type must be one of", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        [Fact]
        public async Task UploadAsync_Fails_WhenFileSizeExceedsTheSliceLimit()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            try
            {
                await using var context = CreateContext(databaseName, tenantContext);
                var commandService = CreateCommandService(context, tenantContext, storageRootPath);
                var fileBytes = new byte[128];
                using var stream = new MemoryStream(fileBytes);

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.UploadAsync(
                    patient.Id,
                    new UploadPatientDocumentCommand(
                        "too-large.pdf",
                        "application/pdf",
                        PatientDocument.MaxFileSizeBytes + 1,
                        stream)));

                Assert.Contains("must not exceed", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        [Fact]
        public async Task UploadAsync_Fails_WhenPatientDoesNotExist()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var (tenantA, _) = await SeedTenantsAsync(databaseName);
            var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);

            try
            {
                await using var context = CreateContext(databaseName, tenantContext);
                var commandService = CreateCommandService(context, tenantContext, storageRootPath);
                var fileBytes = Encoding.UTF8.GetBytes("pdf");
                using var stream = new MemoryStream(fileBytes);

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.UploadAsync(
                    Guid.NewGuid(),
                    new UploadPatientDocumentCommand(
                        "missing-patient.pdf",
                        "application/pdf",
                        fileBytes.LongLength,
                        stream)));

                Assert.Contains("patient is not available", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        [Fact]
        public async Task ListAndDownload_BlockCrossTenantReads()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientA = await SeedPatientAsync(databaseName, tenantA.Id, "Ana", "Lopez");
            var writerTenantContext = CreateTenantContext(actorUserId, tenantA.Id);

            Guid documentId;

            try
            {
                await using (var writerContext = CreateContext(databaseName, writerTenantContext))
                {
                    var commandService = CreateCommandService(writerContext, writerTenantContext, storageRootPath);
                    var fileBytes = Encoding.UTF8.GetBytes("pdf");
                    using var uploadStream = new MemoryStream(fileBytes);
                    var created = await commandService.UploadAsync(
                        patientA.Id,
                        new UploadPatientDocumentCommand(
                            "radiography.pdf",
                            "application/pdf",
                            fileBytes.LongLength,
                            uploadStream));

                    documentId = created.DocumentId;
                }

                var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantB.Id);
                await using var context = CreateContext(databaseName, tenantContext);
                var queryService = CreateQueryService(context, tenantContext, storageRootPath);

                Assert.Null(await queryService.ListActiveByPatientIdAsync(patientA.Id));
                Assert.Null(await queryService.DownloadAsync(patientA.Id, documentId));
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        [Fact]
        public async Task UploadAndRetire_BlockCrossTenantWrites()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var actorUserId = Guid.NewGuid();
            var (tenantA, tenantB) = await SeedTenantsAsync(databaseName);
            var patientB = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            var writerTenantContext = CreateTenantContext(actorUserId, tenantB.Id);
            Guid documentId;

            try
            {
                await using (var writerContext = CreateContext(databaseName, writerTenantContext))
                {
                    var writerCommandService = CreateCommandService(writerContext, writerTenantContext, storageRootPath);
                    var fileBytes = Encoding.UTF8.GetBytes("pdf");
                    using var uploadStream = new MemoryStream(fileBytes);
                    var created = await writerCommandService.UploadAsync(
                        patientB.Id,
                        new UploadPatientDocumentCommand(
                            "radiography.pdf",
                            "application/pdf",
                            fileBytes.LongLength,
                            uploadStream));

                    documentId = created.DocumentId;
                }

                var tenantContext = CreateTenantContext(Guid.NewGuid(), tenantA.Id);
                await using var context = CreateContext(databaseName, tenantContext);
                var commandService = CreateCommandService(context, tenantContext, storageRootPath);
                var bytes = Encoding.UTF8.GetBytes("pdf");
                using var writeStream = new MemoryStream(bytes);

                var uploadException = await Assert.ThrowsAsync<InvalidOperationException>(() => commandService.UploadAsync(
                    patientB.Id,
                    new UploadPatientDocumentCommand(
                        "foreign.pdf",
                        "application/pdf",
                        bytes.LongLength,
                        writeStream)));

                var retireException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    commandService.RetireAsync(patientB.Id, documentId));

                Assert.Contains("patient is not available", uploadException.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("patient is not available", retireException.Message, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        [Fact]
        public async Task PlatformAdminOverride_UploadListDownloadAndRetire_SucceedForPatientScopedDocuments()
        {
            var databaseName = Guid.NewGuid().ToString();
            var storageRootPath = CreateStorageRootPath();
            var actorUserId = Guid.NewGuid();
            var (_, tenantB) = await SeedTenantsAsync(databaseName);
            var patient = await SeedPatientAsync(databaseName, tenantB.Id, "Bruno", "Garcia");
            var tenantContext = CreatePlatformOverrideContext(actorUserId);

            try
            {
                await using var context = CreateContext(databaseName, tenantContext);
                var commandService = CreateCommandService(context, tenantContext, storageRootPath);
                var queryService = CreateQueryService(context, tenantContext, storageRootPath);
                var fileBytes = Encoding.UTF8.GetBytes("platform-pdf");

                using var uploadStream = new MemoryStream(fileBytes);
                var created = await commandService.UploadAsync(
                    patient.Id,
                    new UploadPatientDocumentCommand(
                        "support-radiography.pdf",
                        "application/pdf",
                        fileBytes.LongLength,
                        uploadStream));

                Assert.Equal(patient.Id, created.PatientId);
                Assert.Equal(actorUserId, created.UploadedByUserId);

                var listed = await queryService.ListActiveByPatientIdAsync(patient.Id);

                var listedDocument = Assert.Single(listed!);
                Assert.Equal(created.DocumentId, listedDocument.DocumentId);

                var downloaded = await queryService.DownloadAsync(patient.Id, created.DocumentId);

                Assert.NotNull(downloaded);
                await using var downloadedContentStream = downloaded!.ContentStream;
                using var downloadBuffer = new MemoryStream();
                await downloadedContentStream.CopyToAsync(downloadBuffer);
                Assert.Equal(fileBytes, downloadBuffer.ToArray());

                var retired = await commandService.RetireAsync(patient.Id, created.DocumentId);

                Assert.True(retired);
                Assert.Empty((await queryService.ListActiveByPatientIdAsync(patient.Id))!);
                Assert.Null(await queryService.DownloadAsync(patient.Id, created.DocumentId));
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

        private static IPatientDocumentCommandService CreateCommandService(AppDbContext context, TenantContext tenantContext, string storageRootPath)
        {
            return new PatientDocumentCommandService(
                new EfPatientDocumentRepository(context),
                CreateBinaryStore(storageRootPath),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static IPatientDocumentQueryService CreateQueryService(AppDbContext context, TenantContext tenantContext, string storageRootPath)
        {
            return new PatientDocumentQueryService(
                new EfPatientDocumentRepository(context),
                CreateBinaryStore(storageRootPath),
                new EfPatientRepository(context),
                tenantContext);
        }

        private static IPatientDocumentBinaryStore CreateBinaryStore(string storageRootPath)
        {
            return new LocalPatientDocumentBinaryStore(
                Options.Create(new PatientDocumentStorageOptions
                {
                    RootPath = storageRootPath
                }),
                new TestHostEnvironment(storageRootPath));
        }

        private static TenantContext CreateTenantContext(Guid userId, Guid tenantId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(userId.ToString(), AccessScope.Tenant, isAuthenticated: true, tenantId.ToString());
            return tenantContext;
        }

        private static TenantContext CreatePlatformOverrideContext(Guid userId)
        {
            var tenantContext = new TenantContext();
            tenantContext.SetRequestContext(userId.ToString(), AccessScope.Platform, isAuthenticated: true);
            tenantContext.EnablePlatformOverride();
            return tenantContext;
        }

        private static string CreateStorageRootPath()
        {
            var rootPath = Path.Combine(Path.GetTempPath(), "bigsmile-patient-documents-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(rootPath);
            return rootPath;
        }

        private static void DeleteStorageRootPath(string storageRootPath)
        {
            if (Directory.Exists(storageRootPath))
            {
                Directory.Delete(storageRootPath, recursive: true);
            }
        }

        private static async Task<(Tenant TenantA, Tenant TenantB)> SeedTenantsAsync(string databaseName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var tenantA = new Tenant("Tenant A", "tenant-a");
            tenantA.AddBranch("Branch A");
            var tenantB = new Tenant("Tenant B", "tenant-b");
            tenantB.AddBranch("Branch B");

            context.Tenants.AddRange(tenantA, tenantB);
            await context.SaveChangesAsync();

            return (tenantA, tenantB);
        }

        private static async Task<Patient> SeedPatientAsync(string databaseName, Guid tenantId, string firstName, string lastName)
        {
            await using var context = CreateContext(databaseName, new TenantContext());
            var patient = new Patient(
                tenantId,
                firstName,
                lastName,
                new DateOnly(1991, 2, 14),
                "5551234567",
                $"{firstName.ToLowerInvariant()}@example.com");

            context.Patients.Add(patient);
            await context.SaveChangesAsync();
            return patient;
        }

        private static AppDbContext CreateContext(string databaseName, TenantContext tenantContext)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            return new AppDbContext(options, CreateConfiguration(), tenantContext);
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
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
