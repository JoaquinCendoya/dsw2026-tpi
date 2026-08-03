using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using System.Globalization;

namespace Dsw2026Tpi.Application.Mappings;

public static class AvailabilityMappingExtensions
{
    private static readonly Dictionary<DayOfWeek, string> DiasEnEspanol = new()
    {
        [DayOfWeek.Monday] = "LUNES",
        [DayOfWeek.Tuesday] = "MARTES",
        [DayOfWeek.Wednesday] = "MIÉRCOLES",
        [DayOfWeek.Thursday] = "JUEVES",
        [DayOfWeek.Friday] = "VIERNES",
        [DayOfWeek.Saturday] = "SÁBADO",
        [DayOfWeek.Sunday] = "DOMINGO",
    };

    public static DoctorModel.AvailabilityResponse ToResponse(this AvailabilitySlot entity)
    {
        var day = $"{DiasEnEspanol[entity.SlotDate.DayOfWeek]} {entity.SlotDate:dd/MM}";

        return new DoctorModel.AvailabilityResponse(
            entity.Id,
            day,
            entity.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            entity.EndTime.ToString("HH:mm", CultureInfo.InvariantCulture));
    }
}