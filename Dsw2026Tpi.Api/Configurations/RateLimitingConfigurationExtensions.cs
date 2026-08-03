using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations;

public static class RateLimitingConfigurationExtensions
{
    public static IServiceCollection AddAppRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            void AddFixedWindowPolicy(string policyName, string limitConfigKey, Func<HttpContext, string> partitionKey)
            {
                options.AddPolicy(policyName, context => RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = configuration.GetValue<int>($"RateLimiting:{limitConfigKey}"),
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
            }

            AddFixedWindowPolicy("AdminLoginPolicy", "AdminLoginLimit", GetClientIp);
            AddFixedWindowPolicy("PatientLoginPolicy", "PatientLoginLimit", GetClientIp);
            AddFixedWindowPolicy("AppointmentPolicy", "AppointmentLimit", GetAuthenticatedClient);
            AddFixedWindowPolicy("DefaultPolicy", "DefaultLimit", GetAuthenticatedClient);

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Rate limit excedido. Cliente: {Client}, Ruta: {Path}",
                    GetAuthenticatedClient(context.HttpContext),
                    context.HttpContext.Request.Path);

                var errorResponse = new
                {
                    errorCode = "RATE_LIMIT_EXCEEDED",
                    message = "Too many requests. Please try again later.",
                    details = new[]
                    {
                        new { field = "request", issue = "limit_exceeded" }
                    }
                };

                await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
            };
        });

        return services;
    }

    private static string GetClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetAuthenticatedClient(HttpContext context) =>
        context.User.Identity?.IsAuthenticated == true
            ? context.User.Identity.Name ?? GetClientIp(context)
            : GetClientIp(context);
}
