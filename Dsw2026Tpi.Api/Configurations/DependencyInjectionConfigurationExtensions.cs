using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Validators;
using Dsw2026Tpi.Data.Repositories;
using Dsw2026Tpi.Domain.Interfaces;
using FluentValidation;

namespace Dsw2026Tpi.Api.Configurations;

public static class DependencyInjectionConfigurationExtensions
{
    public static IServiceCollection AddAppDependencies(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ISignInService, SignInService>();
        services.AddSingleton<JwtService>();

        services.AddValidatorsFromAssemblyContaining<SpecialityRequestValidator>();

        return services;
    }
}