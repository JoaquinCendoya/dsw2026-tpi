using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{

    public class DoctorConfiguration : EntityBaseConfiguration<Doctor>
    {
        public override void Configure(EntityTypeBuilder<Doctor> builder)
        {
            base.Configure(builder);

            builder.ToTable("Doctors");

            builder.Property(d => d.Name).IsRequired().HasMaxLength(100);
            builder.Property(d => d.LicenseNumber).HasMaxLength(50).IsRequired(false);

            builder.HasOne(d => d.Specialty)
                   .WithMany()
                   .HasForeignKey(d => d.SpecialtyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}