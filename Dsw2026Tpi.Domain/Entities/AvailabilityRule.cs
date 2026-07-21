using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilityRule : EntityBase
    {
        public Guid DoctorId { get; init; }
        public Doctor Doctor { get; private set; }
        public int Month { get; init; }
        public int Year { get; init; }
        public DayOfWeek DayOfWeek { get; init; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public bool IsActive { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilityRule()
        {
        }
#pragma warning restore CS8618
        #endregion

        public AvailabilityRule(Doctor doctor, int month, int year, DayOfWeek dayOfWeek,
            TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            if (startTime >= endTime)
                throw new ArgumentException("StartTime must be earlier than EndTime.");

            Doctor = doctor;
            DoctorId = doctor.Id;
            Month = month;
            Year = year;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
            IsActive = true;
        }

        public void UpdateRange(TimeOnly startTime, TimeOnly endTime)
        {
            if (startTime >= endTime)
                throw new ArgumentException("StartTime must be earlier than EndTime.");

            StartTime = startTime;
            EndTime = endTime;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}
