using Dsw2026Tpi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

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

        [Timestamp]
        public byte[] RowVersion { get; set; } // control de concurrencia (evitar doble reserva)

        #region Constructor for EF
#pragma warning disable CS8618
        private Appointment()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Appointment(AvailabilitySlot slot, Patient patient, string reason, Guid? id = null) : base(id)
        {
            AvailabilitySlot = slot;
            AvailabilitySlotId = slot.Id;
            Patient = patient;
            PatientId = patient.Id;
            Reason = reason;
            Status = AppointmentStatus.Booked;
        }

        public void Cancel()
        {
            if (Status != AppointmentStatus.Booked)
                throw new InvalidOperationException("Only booked appointments can be cancelled.");

            Status = AppointmentStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
        }

        public void MarkAttended()
        {
            Status = AppointmentStatus.Attended;
            AttendedAt = DateTime.UtcNow;
        }

        public void MarkNoShow()
        {
            Status = AppointmentStatus.NoShow;
        }
    }
}
