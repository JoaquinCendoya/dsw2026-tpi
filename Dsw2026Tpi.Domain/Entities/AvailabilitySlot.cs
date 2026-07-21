using Dsw2026Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilitySlot : EntityBase
    {
        public Guid DoctorId { get; init; }
        public Doctor Doctor { get; private set; }
        public Guid? AvailabilityRuleId { get; init; }
        public AvailabilityRule? AvailabilityRule { get; private set; }
        public DateOnly SlotDate { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public SlotStatus Status { get; private set; }
        public bool IsActive { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilitySlot()
        {
        }
#pragma warning restore CS8618
        #endregion

        public AvailabilitySlot(Doctor doctor, AvailabilityRule rule, DateOnly slotDate,
            TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            Doctor = doctor;
            DoctorId = doctor.Id;
            AvailabilityRule = rule;
            AvailabilityRuleId = rule.Id;
            SlotDate = slotDate;
            StartTime = startTime;
            EndTime = endTime;
            Status = SlotStatus.Available;
            IsActive = true;
        }

        public void Book()
        {
            if (Status != SlotStatus.Available)
                throw new InvalidOperationException("Slot is not available.");
            Status = SlotStatus.Booked;
        }

        public void Release()
        {
            Status = SlotStatus.Available;
        }

        public void Block()
        {
            Status = SlotStatus.Blocked;
        }
    }
}
