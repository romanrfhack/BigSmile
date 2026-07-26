using BigSmile.Api.Authorization;
using BigSmile.Api.Controllers;
using BigSmile.Application;
using BigSmile.Application.Interfaces.Security;
using BigSmile.Infrastructure;
using BigSmile.Infrastructure.Middleware;
using BigSmile.Infrastructure.Options;
using BigSmile.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);
builder.Services.AddSingleton(jwtSettings);

var patientPortalJwtSettings = new PatientPortalJwtSettings(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = PatientPortalAuthenticationDefaults.SelectorScheme;
    options.DefaultAuthenticateScheme = PatientPortalAuthenticationDefaults.SelectorScheme;
    options.DefaultChallengeScheme = PatientPortalAuthenticationDefaults.SelectorScheme;
})
.AddPolicyScheme(
    PatientPortalAuthenticationDefaults.SelectorScheme,
    PatientPortalAuthenticationDefaults.SelectorScheme,
    options =>
    {
        options.ForwardDefaultSelector = context =>
            PatientPortalAuthenticationSchemeSelector.SelectScheme(context.Request.Path);
    })
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
})
.AddJwtBearer(PatientPortalAuthenticationDefaults.PatientBearerScheme, options =>
{
    options.SaveToken = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = patientPortalJwtSettings.Issuer,
        ValidAudience = patientPortalJwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(patientPortalJwtSettings.Secret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            if (context.Principal is null)
            {
                context.Fail("The patient portal token claims are invalid.");
                return;
            }

            if (PatientPortalClaims.TryGetSessionIdentity(
                    context.Principal,
                    out var patientIdentity))
            {
                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<IPatientPortalSessionValidator>();
                if (!await validator.ValidateAsync(
                        patientIdentity,
                        context.HttpContext.RequestAborted))
                {
                    context.Fail("The patient portal session is no longer valid.");
                }

                return;
            }

            if (PatientPortalClaims.TryGetIntakeSessionIdentity(
                    context.Principal,
                    out var intakeIdentity))
            {
                var validator = context.HttpContext.RequestServices
                    .GetRequiredService<IPatientIntakeSessionValidator>();
                if (!await validator.ValidateAsync(
                        intakeIdentity,
                        context.HttpContext.RequestAborted))
                {
                    context.Fail("The patient intake session is no longer valid.");
                }

                return;
            }

            context.Fail("The patient portal token claims are invalid.");
        }
    };
});

builder.Services.AddAuthorization(AuthorizationPolicies.AddPolicies);
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

var activationPermitLimit = ReadBoundedInt(
    builder.Configuration,
    "PatientPortal:RateLimits:ActivationPermitLimit",
    defaultValue: 5,
    minimum: 1,
    maximum: 100);
var loginPermitLimit = ReadBoundedInt(
    builder.Configuration,
    "PatientPortal:RateLimits:LoginPermitLimit",
    defaultValue: 10,
    minimum: 1,
    maximum: 200);
var rateLimitWindowSeconds = ReadBoundedInt(
    builder.Configuration,
    "PatientPortal:RateLimits:WindowSeconds",
    defaultValue: 60,
    minimum: 10,
    maximum: 3_600);
var rateLimitWindow = TimeSpan.FromSeconds(rateLimitWindowSeconds);

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Patient portal request limit reached.",
            Detail = "The request cannot be processed at this time. Try again later."
        }, cancellationToken);
    };

    options.AddPolicy(
        PatientPortalAuthenticationController.ActivationRateLimitPolicy,
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetNormalizedRemoteIp(httpContext),
            factory: _ => CreateFixedWindowOptions(activationPermitLimit, rateLimitWindow)));

    options.AddPolicy(
        PatientPortalAuthenticationController.LoginRateLimitPolicy,
        httpContext => CreateRealmLoginPartition(
            httpContext,
            loginPermitLimit,
            rateLimitWindow));

    options.AddPolicy(
        PatientIntakeAuthenticationController.ActivationRateLimitPolicy,
        httpContext => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetNormalizedRemoteIp(httpContext),
            factory: _ => CreateFixedWindowOptions(activationPermitLimit, rateLimitWindow)));

    options.AddPolicy(
        PatientIntakeAuthenticationController.LoginRateLimitPolicy,
        httpContext => CreateRealmLoginPartition(
            httpContext,
            loginPermitLimit,
            rateLimitWindow));
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        await BigSmile.Infrastructure.Data.DatabaseInitializer.InitializeAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

await BigSmile.Infrastructure.Data.RealPilotUserBootstrapper.BootstrapIfRequestedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();

static RateLimitPartition<string> CreateRealmLoginPartition(
    HttpContext httpContext,
    int permitLimit,
    TimeSpan window)
{
    var tenantRealm = httpContext.Request.RouteValues.TryGetValue(
        "tenantSubdomain",
        out var routeValue)
        ? routeValue?.ToString()?.Trim().ToLowerInvariant() ?? "unknown"
        : "unknown";
    var partitionKey = $"{GetNormalizedRemoteIp(httpContext)}|{tenantRealm}";
    return RateLimitPartition.GetFixedWindowLimiter(
        partitionKey,
        _ => CreateFixedWindowOptions(permitLimit, window));
}

static FixedWindowRateLimiterOptions CreateFixedWindowOptions(int permitLimit, TimeSpan window)
{
    return new FixedWindowRateLimiterOptions
    {
        PermitLimit = permitLimit,
        Window = window,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0,
        AutoReplenishment = true
    };
}

static string GetNormalizedRemoteIp(HttpContext httpContext)
{
    return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

static int ReadBoundedInt(
    IConfiguration configuration,
    string key,
    int defaultValue,
    int minimum,
    int maximum)
{
    var configured = configuration[key];
    if (string.IsNullOrWhiteSpace(configured))
    {
        return defaultValue;
    }

    if (!int.TryParse(configured, out var parsed) || parsed < minimum || parsed > maximum)
    {
        throw new InvalidOperationException(
            $"Configuration '{key}' must be an integer between {minimum} and {maximum}.");
    }

    return parsed;
}
