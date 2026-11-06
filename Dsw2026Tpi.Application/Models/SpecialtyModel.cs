namespace Dsw2026Tpi.Application.Models;

public record SpecialtyModel
{
    public record Request(string Name, string Description);

    public record StatusRequest(bool IsActive);

    public record Response(Guid Id, string Name, string Description, bool IsActive = true);
    
    public record SearchRequest(int PageSize, int PageIndex, string? Name = null, bool Descending = false, bool IncludeInactive = true);
}

