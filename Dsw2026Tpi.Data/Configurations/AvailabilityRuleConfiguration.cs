using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{
    public class AvailabilityRuleConfiguration : EntityBaseConfiguration<AvailabilityRule>
    {
        public override void Configure(EntityTypeBuilder<AvailabilityRule> builder)
        {
            base.Configure(builder);
            builder.ToTable("AvailabilityRules");

            builder.Property(a => a.Month).IsRequired();
            builder.Property(a => a.Year).IsRequired();
            builder.Property(a => a.DayOfWeek).IsRequired();
            builder.Property(a => a.StartTime).IsRequired();
            builder.Property(a => a.EndTime).IsRequired();

            // Relacion 1:N con Doctor
            builder.HasOne(a => a.Doctor)
                   .WithMany()
                   .HasForeignKey(a => a.DoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Constraint UNIQUE exigido en el ERD
            builder.HasIndex(a => new { a.DoctorId, a.Year, a.Month, a.DayOfWeek, a.StartTime, a.EndTime })
                   .IsUnique();

            builder.Property(a => a.Deleted).HasDefaultValue(false);
        }
    }
}
