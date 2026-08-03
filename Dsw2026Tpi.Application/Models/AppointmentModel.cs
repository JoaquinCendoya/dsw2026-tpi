namespace Dsw2026Tpi.Application.Models;

public record AppointmentModel
{
    public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);

    public record PatientDto(long Dni);

    public record AttendanceRequest(bool Attended);

    public record DailyRequest(DateOnly Date, int PageSize, int PageIndex);

    public record SearchRequest(int PageSize, int PageIndex, Guid? SpecialtyId, Guid? DoctorId, long? Dni, DateOnly? Date);

    public record SearchResponse(
        Guid AppointmentsId,
        string AppointmentsStatus,
        PatientSummary Patient,
        DoctorSummary Doctor
    );

    public record PatientSummary(long Dni, string? FullName);

    public record DoctorSummary(Guid DoctorId, string Name, SpecialtySummary Specialty);

    public record SpecialtySummary(Guid SpecialtyId, string Name);
}
