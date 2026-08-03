using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Models;
using Dsw2026Tpi.CrossCutting.Resources;
using System.Net;
using System.Text.Json;

namespace Dsw2026Tpi.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        ErrorResponse error = ex is AppException exApp ?
            exApp.Error :
            new ErrorResponse(nameof(ErrorCodes.UNHANDLED_ERROR), ErrorCodes.UNHANDLED_ERROR);

        var status = ex switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            AuthenticationException => HttpStatusCode.Unauthorized,
            AuthorizationException => HttpStatusCode.Forbidden,
            EntityNotFoundException => HttpStatusCode.NotFound,
            ConflictException or BusinessRuleException => HttpStatusCode.Conflict,
            ConfigurationException => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.InternalServerError,
        };

        if (status >= HttpStatusCode.InternalServerError)
        {
            _logger.LogError(ex, "Error no controlado. ErrorCode: {ErrorCode}, Path: {Path}",
                error.ErrorCode, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Solicitud rechazada. ErrorCode: {ErrorCode}, Status: {Status}, Path: {Path}",
                error.ErrorCode, (int)status, context.Request.Path);
        }

        var result = JsonSerializer.Serialize(error, SerializerOptions);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;
        await context.Response.WriteAsync(result);
    }
}
