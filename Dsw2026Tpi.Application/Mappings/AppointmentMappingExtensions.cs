using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Enums;

namespace Dsw2026Tpi.Application.Mappings;

public static class AppointmentMappingExtensions
{
    private static readonly Dictionary<AppointmentStatus, string> StatusLiterals = new()
    {
        [AppointmentStatus.Booked] = "BOOKED",
        [AppointmentStatus.Cancelled] = "CANCELLED",
        [AppointmentStatus.Attended] = "ATTENDED",
        [AppointmentStatus.NoShow] = "NO_SHOW",
    };

    public static AppointmentModel.SearchResponse ToSearchResponse(this Appointment entity)
    {
        var doctor = entity.AvailabilitySlot.AvailabilityRule.Doctor;
        var patient = entity.Patient;

        return new AppointmentModel.SearchResponse(
            entity.Id,
            StatusLiterals[entity.Status],
            new AppointmentModel.PatientSummary(long.Parse(patient.Dni), patient.FullName),
            new AppointmentModel.DoctorSummary(
                doctor.Id,
                doctor.Name,
                new AppointmentModel.SpecialtySummary(doctor.Specialty!.Id, doctor.Specialty.Name)));
    }
}
