using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Enums;
using System;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Appointment : EntityBase
    {
        public Guid AvailabilitySlotId { get; init; }
        public AvailabilitySlot AvailabilitySlot { get; private set; }
        public Guid PatientId { get; init; }
        public Patient Patient { get; private set; }
        public string Reason { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public DateTime? AttendedAt { get; private set; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Appointment()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Appointment(AvailabilitySlot slot, Patient patient, string reason, Guid? id = null) : base(id)
        {
            if (string.IsNullOrWhiteSpace(reason) || reason.Length < 5)
                throw new ValidationException(ErrorCodes.INVALID_REASON, nameof(ErrorCodes.INVALID_REASON))
                    .WithDetail("reason", "min_length_5");

            AvailabilitySlot = slot ?? throw new ArgumentNullException(nameof(slot));
            AvailabilitySlotId = slot.Id;

            Patient = patient ?? throw new ArgumentNullException(nameof(patient));
            PatientId = patient.Id;

            Reason = reason;
            Status = AppointmentStatus.Booked;
        }

        public void Cancel() => TransitionFromBooked(AppointmentStatus.Cancelled, () => CancelledAt = DateTime.UtcNow);

        public void MarkAttended() => TransitionFromBooked(AppointmentStatus.Attended, () => AttendedAt = DateTime.UtcNow);

        public void MarkNoShow() => TransitionFromBooked(AppointmentStatus.NoShow, () => { });

        private void TransitionFromBooked(AppointmentStatus newStatus, Action onTransition)
        {
            if (Status != AppointmentStatus.Booked)
                throw new BusinessRuleException(
                        string.Format(ErrorCodes.INVALID_APPOINTMENT_STATUS, Status),
                        nameof(ErrorCodes.INVALID_APPOINTMENT_STATUS))
                    .WithDetail("status", Status.ToString());

            Status = newStatus;
            onTransition();
        }
    }
}
