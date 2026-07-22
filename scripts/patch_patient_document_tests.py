from pathlib import Path

path = Path(__file__).resolve().parents[1] / "backend/tests/BigSmile.IntegrationTests/PatientDocuments/PatientDocumentServicesTests.cs"
text = path.read_text(encoding="utf-8")

replacements = [
    (
        'Encoding.UTF8.GetBytes("pdf-binary")',
        'Encoding.UTF8.GetBytes("%PDF-1.7\\npdf-binary")',
    ),
    (
        'Encoding.UTF8.GetBytes("pdf")',
        'Encoding.UTF8.GetBytes("%PDF-1.7\\npdf")',
    ),
    (
        'Encoding.UTF8.GetBytes("platform-pdf")',
        'Encoding.UTF8.GetBytes("%PDF-1.7\\nplatform-pdf")',
    ),
]

for old, new in replacements:
    count = text.count(old)
    print(f"Replacing {count} occurrence(s) of {old}")
    text = text.replace(old, new)

marker = """        [Fact]
        public async Task UploadAsync_Fails_WhenFileSizeExceedsTheSliceLimit()
"""
new_test = """        [Fact]
        public async Task UploadAsync_Fails_WhenContentDoesNotMatchDeclaredType()
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
                var bytes = Encoding.UTF8.GetBytes("<html>not a PDF</html>");
                using var stream = new MemoryStream(bytes);

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => commandService.UploadAsync(
                    patient.Id,
                    new UploadPatientDocumentCommand(
                        "spoofed.pdf",
                        "application/pdf",
                        bytes.LongLength,
                        stream)));

                Assert.Contains("does not match declared content type", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Empty(context.PatientDocuments);
                Assert.Empty(Directory.GetFiles(storageRootPath, "*", SearchOption.AllDirectories));
            }
            finally
            {
                DeleteStorageRootPath(storageRootPath);
            }
        }

"""

if "UploadAsync_Fails_WhenContentDoesNotMatchDeclaredType" not in text:
    if text.count(marker) != 1:
        raise RuntimeError("Patient document size-test insertion marker was not found exactly once")
    text = text.replace(marker, new_test + marker, 1)
else:
    print("Mismatch test already present")

path.write_text(text, encoding="utf-8")
print("Patched PatientDocumentServicesTests.cs")
