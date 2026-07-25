using BigSmile.Api.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace BigSmile.UnitTests.Authorization
{
    public sealed class PatientPortalAuthenticationSchemeSelectorTests
    {
        [Theory]
        [InlineData("/api/patient-portal/auth/me")]
        [InlineData("/api/patient-portal/intake")]
        public void SelectScheme_UsesPatientBearerForPatientPortalRoutes(string path)
        {
            Assert.Equal(
                PatientPortalAuthenticationDefaults.PatientBearerScheme,
                PatientPortalAuthenticationSchemeSelector.SelectScheme(new PathString(path)));
        }

        [Theory]
        [InlineData("/api/auth/me")]
        [InlineData("/api/patients/5dd69fee-c28e-4c85-91db-57d9933a3063")]
        [InlineData("/api/patients/5dd69fee-c28e-4c85-91db-57d9933a3063/portal-account/recovery")]
        public void SelectScheme_PreservesStaffBearerForStaffRoutes(string path)
        {
            Assert.Equal(
                JwtBearerDefaults.AuthenticationScheme,
                PatientPortalAuthenticationSchemeSelector.SelectScheme(new PathString(path)));
        }
    }
}
