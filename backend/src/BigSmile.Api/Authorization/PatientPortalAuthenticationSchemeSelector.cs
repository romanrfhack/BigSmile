using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;

namespace BigSmile.Api.Authorization
{
    public static class PatientPortalAuthenticationSchemeSelector
    {
        public static string SelectScheme(PathString requestPath)
        {
            return requestPath.StartsWithSegments(PatientPortalAuthenticationDefaults.PatientPathPrefix)
                ? PatientPortalAuthenticationDefaults.PatientBearerScheme
                : JwtBearerDefaults.AuthenticationScheme;
        }
    }
}
