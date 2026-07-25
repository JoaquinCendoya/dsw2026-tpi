using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

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

            // Relacion obligatoria con AvailabilityRule
            builder.HasOne(a => a.AvailabilityRule)
                   .WithMany()
                   .HasForeignKey(a => a.AvailabilityRuleId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Constraint UNIQUE para evitar colision fisica de bloques generados
            builder.HasIndex(a => new { a.AvailabilityRuleId, a.SlotDate, a.StartTime })
                   .IsUnique();

            builder.Property(a => a.Deleted).HasDefaultValue(false);
        }
    }
}
