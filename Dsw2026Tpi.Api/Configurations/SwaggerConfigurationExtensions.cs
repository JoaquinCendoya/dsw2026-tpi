using Microsoft.OpenApi;
using System.Reflection;

namespace Dsw2026Tpi.Api.Configurations;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(o =>
        {
            const string schemeId = "Bearer";
            o.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Desarollo de Software 2026",
                Version = "v1",
                Description = "API REST para el Sistema de Turnos Médicos - Trabajo Práctico Integrador 2026"
            });
            o.AddSecurityDefinition(schemeId, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Ingrese el token JWT con el prefijo 'Bearer ' (ej: Bearer eyJhbG...)",
                Type = SecuritySchemeType.ApiKey
            });
            o.AddSecurityRequirement(doc =>
            {
                return new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference(schemeId, doc),
                        new List<string>()
                    }
                };
            });

            o.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                o.IncludeXmlComments(xmlPath);
            }
        });
        return services;
    }
}
