using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class OdontogramConfiguration : IEntityTypeConfiguration<Odontogram>
    {
        public void Configure(EntityTypeBuilder<Odontogram> builder)
        {
            builder.ToTable("Odontograms");

            builder.HasKey(odontogram => odontogram.Id);

            builder.Property(odontogram => odontogram.CreatedAtUtc)
                .IsRequired();

            builder.Property(odontogram => odontogram.CreatedByUserId)
                .IsRequired();

            builder.Property(odontogram => odontogram.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(odontogram => odontogram.LastUpdatedByUserId)
                .IsRequired();

            builder.HasIndex(odontogram => new { odontogram.TenantId, odontogram.PatientId })
                .IsUnique();

            builder.HasOne(odontogram => odontogram.Tenant)
                .WithMany()
                .HasForeignKey(odontogram => odontogram.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(odontogram => odontogram.Patient)
                .WithMany()
                .HasForeignKey(odontogram => odontogram.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(odontogram => odontogram.Teeth)
                .WithOne(tooth => tooth.Odontogram)
                .HasForeignKey(tooth => tooth.OdontogramId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
