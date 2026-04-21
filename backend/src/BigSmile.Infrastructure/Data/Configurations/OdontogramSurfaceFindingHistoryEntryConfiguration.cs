using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class OdontogramSurfaceFindingHistoryEntryConfiguration : IEntityTypeConfiguration<OdontogramSurfaceFindingHistoryEntry>
    {
        public void Configure(EntityTypeBuilder<OdontogramSurfaceFindingHistoryEntry> builder)
        {
            builder.ToTable("OdontogramSurfaceFindingHistoryEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.ToothCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(entry => entry.SurfaceCode)
                .IsRequired()
                .HasMaxLength(1);

            builder.Property(entry => entry.FindingType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(entry => entry.EntryType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(entry => entry.Summary)
                .IsRequired()
                .HasMaxLength(OdontogramSurfaceFindingHistoryEntry.SummaryMaxLength);

            builder.Property(entry => entry.ChangedAtUtc)
                .IsRequired();

            builder.Property(entry => entry.ChangedByUserId)
                .IsRequired();

            builder.Property(entry => entry.ReferenceFindingId);

            builder.HasIndex(entry => new { entry.OdontogramId, entry.ToothCode, entry.SurfaceCode, entry.ChangedAtUtc });
        }
    }
}
