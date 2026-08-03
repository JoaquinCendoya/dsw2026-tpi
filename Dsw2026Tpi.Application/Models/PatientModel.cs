namespace Dsw2026Tpi.Application.Models;

public record PatientModel
{
    public record Response(Guid Id, string Dni, string? FullName, string? Phone);
}
