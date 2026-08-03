namespace Dsw2026Tpi.Application.Models;

public record AppointmentModel
{
    public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);

    public record PatientDto(long Dni);

    public record AttendanceRequest(bool Attended);

    public record DailyRequest(DateOnly Date, int PageSize, int PageIndex);

    public record SearchResponse(
        Guid Id,
        string Specialty,
        string Doctor,
        DateTime AvailableTime,
        string Status
    );
    public record SearchRequest(int PageSize, int PageIndex, Guid? SpecialtyId, Guid? DoctorId, long? Dni, DateOnly? Date);
}

