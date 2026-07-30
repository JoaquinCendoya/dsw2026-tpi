using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        public record Request(Guid DoctorId, IEnumerable<DaySchedule> Days);

        public record DaySchedule(string Day, string StartTime, string EndTime);
    }
}
