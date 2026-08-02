using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations
{

    public class PatientConfiguration : EntityBaseConfiguration<Patient>
    {
        public override void Configure(EntityTypeBuilder<Patient> builder)
        {
            base.Configure(builder);

            builder.ToTable("Patients");

            builder.Property(p => p.UserId).IsRequired();
            builder.HasIndex(p => p.UserId).IsUnique();

            builder.Property(p => p.Dni).IsRequired().HasMaxLength(10);
            builder.HasIndex(p => p.Dni).IsUnique();

            builder.Property(p => p.FullName).HasMaxLength(100).IsRequired(false);
            builder.Property(p => p.Phone).HasMaxLength(20).IsRequired(false);
        }
    }
}
