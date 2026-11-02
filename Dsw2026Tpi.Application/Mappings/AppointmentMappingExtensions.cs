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

        // Validación y parseo seguro del DNI del paciente (asegurando que cumpla con el formato numérico requerido)
        long dniValue = 0;
        if (!string.IsNullOrEmpty(patient.Dni) && long.TryParse(patient.Dni, out var parsedDni))
        {
            dniValue = parsedDni;
        }

        return new AppointmentModel.SearchResponse(
            entity.Id,
            entity.Status.ToString(), // O el literal de estado correspondiente
            new AppointmentModel.PatientSummary(
                dniValue,
                !string.IsNullOrWhiteSpace(patient.FullName) ? patient.FullName : "Sin nombre"
            ),
            new AppointmentModel.DoctorSummary(
                doctor.Id,
                doctor.Name,
                new AppointmentModel.SpecialtySummary(
                    doctor.Specialty!.Id,
                    doctor.Specialty.Name
                )
            )
        );
    }
}
