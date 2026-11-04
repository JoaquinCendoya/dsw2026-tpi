using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Identity;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Dsw2026Tpi.Tests;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_IncluyeClaimDniEnElTokenDelPaciente()
    {
        // Arrange
        var jwtService = new JwtService(CreateConfiguration());

        // Act
        var token = jwtService.GenerateToken("paciente@example.com", Roles.Patient, "30123456");
        var decodedToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        decodedToken.Subject.Should().Be("paciente@example.com");
        decodedToken.Claims.Should().ContainSingle(claim =>
            claim.Type == IdentityClaims.PatientDni && claim.Value == "30123456");
    }

    [Fact]
    public void GenerateToken_FallaSiElPacienteNoTieneDni()
    {
        // Arrange
        var jwtService = new JwtService(CreateConfiguration());

        // Act
        var action = () => jwtService.GenerateToken("paciente@example.com", Roles.Patient);

        // Assert
        action.Should().Throw<ArgumentException>();
    }

    private static IConfiguration CreateConfiguration()
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-signing-key-that-is-at-least-thirty-two-bytes-long",
            ["Jwt:Issuer"] = "Dsw2026Tpi.Tests",
            ["Jwt:Audience"] = "Dsw2026Tpi.Tests",
            ["Jwt:ExpiresInMinutes"] = "15"
        };

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
