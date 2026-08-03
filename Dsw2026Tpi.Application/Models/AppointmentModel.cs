namespace Dsw2026Tpi.Application.Models
{
    public record AppointmentModel
    {
        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);

        public record PatientDto(long Dni);
        
        public record AttendanceRequest(bool Attended);

        public record SearchResponse(
            Guid Id,
            string Specialty,
            string Doctor,
            DateTime AvailableTime,
            string Status
        );
    }
}
