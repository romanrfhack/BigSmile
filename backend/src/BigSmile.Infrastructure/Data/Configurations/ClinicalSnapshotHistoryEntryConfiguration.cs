using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalSnapshotHistoryEntryConfiguration : IEntityTypeConfiguration<ClinicalSnapshotHistoryEntry>
    {
        public void Configure(EntityTypeBuilder<ClinicalSnapshotHistoryEntry> builder)
        {
            builder.ToTable("ClinicalSnapshotHistoryEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.EntryType)
                .IsRequired();

            builder.Property(entry => entry.Section)
                .IsRequired();

            builder.Property(entry => entry.Summary)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(entry => entry.ChangedAtUtc)
                .IsRequired();

            builder.Property(entry => entry.ChangedByUserId)
                .IsRequired();

            builder.HasIndex(entry => new { entry.ClinicalRecordId, entry.ChangedAtUtc });
        }
    }
}
