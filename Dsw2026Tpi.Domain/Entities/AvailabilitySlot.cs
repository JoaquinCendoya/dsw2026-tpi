using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Enums;
using System;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilitySlot : EntityBase
    {
        public Guid AvailabilityRuleId { get; init; }
        public AvailabilityRule AvailabilityRule { get; private set; }
        public DateOnly SlotDate { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public SlotStatus Status { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilitySlot()
        {
        }
#pragma warning restore CS8618
        #endregion

        public AvailabilitySlot(AvailabilityRule rule, DateOnly slotDate,
            TimeOnly startTime, TimeOnly endTime, Guid? id = null) : base(id)
        {
            AvailabilityRule = rule ?? throw new ArgumentNullException(nameof(rule));
            AvailabilityRuleId = rule.Id;
            SlotDate = slotDate;
            StartTime = startTime;
            EndTime = endTime;
            Status = SlotStatus.Available;
        }

        public void Book()
        {
            if (Status != SlotStatus.Available)
                throw new BusinessRuleException(
                        string.Format(ErrorCodes.SLOT_NOT_AVAILABLE, Status),
                        nameof(ErrorCodes.SLOT_NOT_AVAILABLE))
                    .WithDetail("status", Status.ToString());

            Status = SlotStatus.Booked;
        }

        public void Release()
        {
            // Opcional: Podría agregar validación para evitar liberar turnos que no estén en estado Booked o Blocked.
            Status = SlotStatus.Available;
        }

        public void Block()
        {
            Status = SlotStatus.Blocked;
        }
    }
}