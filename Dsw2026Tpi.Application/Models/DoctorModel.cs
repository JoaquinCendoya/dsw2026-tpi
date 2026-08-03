namespace Dsw2026Tpi.Application.Models;

public record DoctorModel
{
    public record Request(string Name, string LicenseNumber, Guid SpecialtyId);

    public record Response(Guid Id, string Name, string LicenseNumber, SpecialtyDto? Specialty);

    public record SpecialtyDto(Guid? Id, string? Name);

    // Requerido para listar los horarios generados
    public record AvailabilityResponse(Guid Id, string Day, string StartTime, string EndTime);

    public record SearchRequest(int PageSize, int PageIndex, string? Name = null, bool Descending = false);

}