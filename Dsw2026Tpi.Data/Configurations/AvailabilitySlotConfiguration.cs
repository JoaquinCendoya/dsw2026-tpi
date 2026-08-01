using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilitySlotConfiguration : EntityBaseConfiguration<AvailabilitySlot>
    {
        public override void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
        {
            base.Configure(builder);
            builder.ToTable("AvailabilitySlots");

            builder.Property(a => a.SlotDate).IsRequired();
            builder.Property(a => a.StartTime).IsRequired();
            builder.Property(a => a.EndTime).IsRequired();
            builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

            // 1. Control de concurrencia optimista
            builder.Property(a => a.RowVersion).IsRowVersion();

            // 2. Corrección de restricción referencial (Previene borrado en cascada)
            builder.HasOne(a => a.AvailabilityRule)
                   .WithMany()
                   .HasForeignKey(a => a.AvailabilityRuleId)
                   .OnDelete(DeleteBehavior.Restrict);

            // 3. Índice único con filtro para borrado lógico
            builder.HasIndex(a => new { a.AvailabilityRuleId, a.SlotDate, a.StartTime })
                   .IsUnique()
                   .HasFilter("[Deleted] = 0");

            builder.Property(a => a.Deleted).HasDefaultValue(false);
        }
    }
}
