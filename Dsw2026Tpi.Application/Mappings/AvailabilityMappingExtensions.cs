using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using System.Globalization;

namespace Dsw2026Tpi.Application.Mappings;

public static class AvailabilityMappingExtensions
{
    public static DoctorModel.AvailabilityResponse ToResponse(this AvailabilitySlot entity)
    {
        return new DoctorModel.AvailabilityResponse(
            entity.Id,
            entity.SlotDate.DayOfWeek.ToString(),
            entity.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture),
            entity.EndTime.ToString("HH:mm", CultureInfo.InvariantCulture));
    }
}