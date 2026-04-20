using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalDiagnosisConfiguration : IEntityTypeConfiguration<ClinicalDiagnosis>
    {
        public void Configure(EntityTypeBuilder<ClinicalDiagnosis> builder)
        {
            builder.ToTable("ClinicalDiagnoses");

            builder.HasKey(diagnosis => diagnosis.Id);

            builder.Property(diagnosis => diagnosis.DiagnosisText)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(diagnosis => diagnosis.Notes)
                .HasMaxLength(500);

            builder.Property(diagnosis => diagnosis.Status)
                .IsRequired();

            builder.Property(diagnosis => diagnosis.CreatedAtUtc)
                .IsRequired();

            builder.Property(diagnosis => diagnosis.CreatedByUserId)
                .IsRequired();

            builder.HasIndex(diagnosis => new { diagnosis.ClinicalRecordId, diagnosis.Status, diagnosis.CreatedAtUtc });
        }
    }
}
