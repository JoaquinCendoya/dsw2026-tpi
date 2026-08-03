using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Mappings;

public static class AppointmentMappingExtensions
{
    public static AppointmentModel.SearchResponse ToSearchResponse(this Appointment entity)
    {
        var doctor = entity.AvailabilitySlot.AvailabilityRule.Doctor;
        var patient = entity.Patient;

        return new AppointmentModel.SearchResponse(
            entity.Id,
            entity.Status.ToString(),
            new AppointmentModel.PatientSummary(long.Parse(patient.Dni), patient.FullName),
            new AppointmentModel.DoctorSummary(
                doctor.Id,
                doctor.Name,
                new AppointmentModel.SpecialtySummary(doctor.Specialty!.Id, doctor.Specialty.Name)));
    }
}
