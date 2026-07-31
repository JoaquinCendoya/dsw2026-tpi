using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Mappings;

public static class AppointmentMappingExtensions
{
    public static AppointmentModel.SearchResponse ToSearchResponse(this Appointment entity)
    {
        var rule = entity.AvailabilitySlot.AvailabilityRule;
        var doctor = rule.Doctor;

        var availableTime = entity.AvailabilitySlot.SlotDate.ToDateTime(entity.AvailabilitySlot.StartTime);

        return new AppointmentModel.SearchResponse(
            entity.Id,
            doctor.Speciality?.Name ?? string.Empty,
            doctor.Name,
            availableTime,
            entity.Status.ToString());
    }
}