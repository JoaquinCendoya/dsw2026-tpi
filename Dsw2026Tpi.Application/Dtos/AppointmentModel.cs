using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AppointmentModel
    {
        public record Request(Guid DoctorId, Guid AvailabilitySlotId, PatientDto Patient, string Reason);

        public record PatientDto(long Dni);

        public record SearchResponse(
            Guid Id,
            string Speciality,
            string Doctor,
            DateTime AvailableTime,
            string Status
        );
    }
}
