using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class OdontogramSurfaceFindingConfiguration : IEntityTypeConfiguration<OdontogramSurfaceFinding>
    {
        public void Configure(EntityTypeBuilder<OdontogramSurfaceFinding> builder)
        {
            builder.ToTable("OdontogramSurfaceFindings");

            builder.HasKey(finding => finding.Id);

            builder.Property(finding => finding.ToothCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(finding => finding.SurfaceCode)
                .IsRequired()
                .HasMaxLength(1);

            builder.Property(finding => finding.FindingType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(finding => finding.CreatedAtUtc)
                .IsRequired();

            builder.Property(finding => finding.CreatedByUserId)
                .IsRequired();

            builder.HasIndex(finding => new { finding.OdontogramId, finding.ToothCode, finding.SurfaceCode });

            builder.HasIndex(finding => new { finding.OdontogramId, finding.ToothCode, finding.SurfaceCode, finding.FindingType })
                .IsUnique();
        }
    }
}
