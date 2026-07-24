using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{
    public class SpecialityConfiguration : EntityBaseConfiguration<Speciality>
    {
        public override void Configure(EntityTypeBuilder<Speciality> builder)
        {
            base.Configure(builder);

            builder.ToTable("Specialities");

            builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
            builder.HasIndex(s => s.Name).IsUnique();

            builder.Property(s => s.Description).IsRequired().HasMaxLength(100);
        }
    }
}