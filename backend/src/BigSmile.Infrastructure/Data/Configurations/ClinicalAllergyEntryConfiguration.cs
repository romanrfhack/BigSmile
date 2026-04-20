using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalAllergyEntryConfiguration : IEntityTypeConfiguration<ClinicalAllergyEntry>
    {
        public void Configure(EntityTypeBuilder<ClinicalAllergyEntry> builder)
        {
            builder.ToTable("ClinicalRecordAllergies");

            builder.HasKey(allergy => allergy.Id);

            builder.Property(allergy => allergy.Substance)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(allergy => allergy.ReactionSummary)
                .HasMaxLength(500);

            builder.Property(allergy => allergy.Notes)
                .HasMaxLength(500);

            builder.HasIndex(allergy => new { allergy.ClinicalRecordId, allergy.Substance })
                .IsUnique();
        }
    }
}
