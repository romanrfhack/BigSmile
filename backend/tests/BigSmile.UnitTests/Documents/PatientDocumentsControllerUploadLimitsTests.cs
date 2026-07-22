using System.Reflection;
using BigSmile.Api.Controllers;
using BigSmile.Domain.Entities;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BigSmile.UnitTests.Documents
{
    public sealed class PatientDocumentsControllerUploadLimitsTests
    {
        [Fact]
        public void Upload_HasBoundedRequestAndMultipartLimits()
        {
            const long expectedLimit = PatientDocument.MaxFileSizeBytes + (1024 * 1024);
            var uploadMethod = typeof(PatientDocumentsController).GetMethod(
                nameof(PatientDocumentsController.Upload),
                BindingFlags.Instance | BindingFlags.Public)
                ?? throw new InvalidOperationException("PatientDocumentsController.Upload was not found.");

            var requestSizeLimit = uploadMethod.GetCustomAttribute<RequestSizeLimitAttribute>();
            var formLimits = uploadMethod.GetCustomAttribute<RequestFormLimitsAttribute>();

            var requestSizeMetadata = Assert.IsAssignableFrom<IRequestSizeLimitMetadata>(requestSizeLimit);
            Assert.NotNull(formLimits);
            Assert.Equal(expectedLimit, requestSizeMetadata.MaxRequestBodySize);
            Assert.Equal(expectedLimit, formLimits!.MultipartBodyLengthLimit);
        }
    }
}
