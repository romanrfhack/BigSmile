using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
    {
        public void Configure(EntityTypeBuilder<ClinicalNote> builder)
        {
            builder.ToTable("ClinicalNotes");

            builder.HasKey(note => note.Id);

            builder.Property(note => note.NoteText)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(note => note.CreatedAtUtc)
                .IsRequired();

            builder.Property(note => note.CreatedByUserId)
                .IsRequired();

            builder.HasIndex(note => new { note.ClinicalRecordId, note.CreatedAtUtc });
        }
    }
}
