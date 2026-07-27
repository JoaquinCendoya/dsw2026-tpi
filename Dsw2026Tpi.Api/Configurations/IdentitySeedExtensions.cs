using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Data.Identity;
using Microsoft.AspNetCore.Identity;

namespace Dsw2026Tpi.Api.Configurations;

public static class IdentitySeedExtensions
{
    public static async Task UseIdentitySeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();
        var logger = services.GetRequiredService<ILogger<Program>>();

        
        string[] roles = [Roles.Administrator, Roles.Patient];
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Rol creado durante el seed: {Role}", roleName);
            }
        }

        
        var adminEmail = configuration["AdminSeed:Email"]
            ?? throw new ArgumentNullException("AdminSeed:Email");
        var adminPassword = configuration["AdminSeed:Password"]
            ?? throw new ArgumentNullException("AdminSeed:Password");

        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
        if (existingAdmin is null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Roles.Administrator);
                logger.LogInformation("Usuario Administrador inicial creado: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("No se pudo crear el usuario Administrador inicial: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}