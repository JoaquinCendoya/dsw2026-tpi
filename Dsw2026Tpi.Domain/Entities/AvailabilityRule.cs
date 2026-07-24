using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using System;

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
            ValidateRange(startTime, endTime);

            Doctor = doctor ?? throw new ArgumentNullException(nameof(doctor));
            DoctorId = doctor.Id;
            Month = month;
            Year = year;
            DayOfWeek = dayOfWeek;
            StartTime = startTime;
            EndTime = endTime;
        }

        public void UpdateRange(TimeOnly startTime, TimeOnly endTime)
        {
            ValidateRange(startTime, endTime);
            StartTime = startTime;
            EndTime = endTime;
        }

        private static void ValidateRange(TimeOnly startTime, TimeOnly endTime)
        {
            if (startTime >= endTime)
                throw new BusinessRuleException(ErrorCodes.INVALID_TIME_RANGE, nameof(ErrorCodes.INVALID_TIME_RANGE))
                    .WithDetail("startTime", "must_be_before_endTime");
        }
    }
}