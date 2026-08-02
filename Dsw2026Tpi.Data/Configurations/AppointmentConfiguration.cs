using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AppointmentConfiguration : EntityBaseConfiguration<Appointment>
    {
        public override void Configure(EntityTypeBuilder<Appointment> builder)
        {
            base.Configure(builder);
            builder.ToTable("Appointments");

            builder.Property(a => a.Reason).IsRequired().HasMaxLength(200);
            builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(a => a.CancelledAt).IsRequired(false);
            builder.Property(a => a.AttendedAt).IsRequired(false);

            // Control de concurrencia nativo (RN03): Un slot solo puede tener una cita asociada
            builder.HasOne(a => a.AvailabilitySlot)
                   .WithMany()
                   .HasForeignKey(a => a.AvailabilitySlotId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Control de concurrencia nativo (RN03): Un slot solo puede tener una cita ACTIVA asociada
            builder.HasIndex(a => a.AvailabilitySlotId)
                   .IsUnique()
                   .HasFilter("[Status] = 'Booked' AND [Deleted] = 0");

            // Relacion con Paciente
            builder.HasOne(a => a.Patient)
                   .WithMany()
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.Deleted).HasDefaultValue(false);
        }
    }
}
