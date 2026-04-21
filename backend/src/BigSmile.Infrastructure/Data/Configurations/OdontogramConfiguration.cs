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

            builder.HasMany(odontogram => odontogram.Surfaces)
                .WithOne(surface => surface.Odontogram)
                .HasForeignKey(surface => surface.OdontogramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(odontogram => odontogram.SurfaceFindings)
                .WithOne(finding => finding.Odontogram)
                .HasForeignKey(finding => finding.OdontogramId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(odontogram => odontogram.SurfaceFindingHistoryEntries)
                .WithOne(entry => entry.Odontogram)
                .HasForeignKey(entry => entry.OdontogramId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
