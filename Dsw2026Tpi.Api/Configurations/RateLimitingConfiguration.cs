using Microsoft.AspNetCore.RateLimiting;

namespace Dsw2026Tpi.Api.Configurations;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddAppRateLimiter(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("AppointmentPolicy", opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:AppointmentLimit");
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });
            options.AddFixedWindowLimiter("AdminLoginPolicy", opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:AdminLoginLimit");
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("PatientLoginPolicy", opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:PatientLoginLimit");
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("DefaultPolicy", opt =>
            {
                opt.PermitLimit = configuration.GetValue<int>("RateLimiting:DefaultLimit");
                opt.Window = TimeSpan.FromMinutes(1);
                opt.QueueLimit = 0;
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning("Rate limit excedido para la IP: {IpAddress} en la ruta: {Path}",
                    context.HttpContext.Connection.RemoteIpAddress,
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
}

