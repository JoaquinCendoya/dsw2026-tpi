namespace Dsw2026Tpi.Application.Dtos;

public record AvailabilityModel
{
    public record Request(string Day, TimeSpan StartTime, TimeSpan EndTime);

    public record Response(DateTime Date, TimeSpan StartTime, TimeSpan EndTime);
}
