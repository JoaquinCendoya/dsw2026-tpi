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
        var doctor = entity.AvailabilitySlot?.AvailabilityRule?.Doctor;
        var patient = entity.Patient;

        // Validación y parseo seguro del DNI del paciente (asegurando que cumpla con el formato numérico requerido)
        long dniValue = 0;
        if (!string.IsNullOrEmpty(patient?.Dni) && long.TryParse(patient.Dni, out var parsedDni))
        {
            dniValue = parsedDni;
        }

        var slotDate = entity.AvailabilitySlot?.SlotDate;
        var startTime = entity.AvailabilitySlot?.StartTime;
        var statusStr = StatusLiterals.TryGetValue(entity.Status, out var lit) ? lit : entity.Status.ToString().ToUpperInvariant();

        var doctorId = doctor?.Id ?? Guid.Empty;
        var doctorName = doctor?.Name ?? "Médico no asignado";
        var specialtyId = doctor?.Specialty?.Id ?? Guid.Empty;
        var specialtyName = doctor?.Specialty?.Name ?? "Sin especialidad";

        return new AppointmentModel.SearchResponse(
            entity.Id,
            statusStr,
            new AppointmentModel.PatientSummary(
                dniValue,
                patient?.FullName
            ),
            new AppointmentModel.DoctorSummary(
                doctorId,
                doctorName,
                new AppointmentModel.SpecialtySummary(
                    specialtyId,
                    specialtyName
                )
            ),
            slotDate,
            startTime
        );
    }
}
