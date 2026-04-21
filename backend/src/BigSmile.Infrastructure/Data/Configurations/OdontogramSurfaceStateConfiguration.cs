using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class OdontogramSurfaceStateConfiguration : IEntityTypeConfiguration<OdontogramSurfaceState>
    {
        public void Configure(EntityTypeBuilder<OdontogramSurfaceState> builder)
        {
            builder.ToTable("OdontogramSurfaceStates");

            builder.HasKey(surface => surface.Id);

            builder.Property(surface => surface.ToothCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(surface => surface.SurfaceCode)
                .IsRequired()
                .HasMaxLength(1);

            builder.Property(surface => surface.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(surface => surface.UpdatedAtUtc)
                .IsRequired();

            builder.Property(surface => surface.UpdatedByUserId)
                .IsRequired();

            builder.HasIndex(surface => new { surface.OdontogramId, surface.ToothCode, surface.SurfaceCode })
                .IsUnique();
        }
    }
}
