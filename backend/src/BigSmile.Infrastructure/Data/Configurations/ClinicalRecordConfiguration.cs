using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalRecordConfiguration : IEntityTypeConfiguration<ClinicalRecord>
    {
        public void Configure(EntityTypeBuilder<ClinicalRecord> builder)
        {
            builder.HasKey(clinicalRecord => clinicalRecord.Id);

            builder.Property(clinicalRecord => clinicalRecord.MedicalBackgroundSummary)
                .HasMaxLength(2000);

            builder.Property(clinicalRecord => clinicalRecord.CurrentMedicationsSummary)
                .HasMaxLength(2000);

            builder.Property(clinicalRecord => clinicalRecord.CreatedAtUtc)
                .IsRequired();

            builder.Property(clinicalRecord => clinicalRecord.CreatedByUserId)
                .IsRequired();

            builder.Property(clinicalRecord => clinicalRecord.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(clinicalRecord => clinicalRecord.LastUpdatedByUserId)
                .IsRequired();

            builder.HasIndex(clinicalRecord => new { clinicalRecord.TenantId, clinicalRecord.PatientId })
                .IsUnique();

            builder.HasOne(clinicalRecord => clinicalRecord.Tenant)
                .WithMany()
                .HasForeignKey(clinicalRecord => clinicalRecord.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(clinicalRecord => clinicalRecord.Patient)
                .WithMany()
                .HasForeignKey(clinicalRecord => clinicalRecord.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(clinicalRecord => clinicalRecord.Allergies)
                .WithOne(allergy => allergy.ClinicalRecord)
                .HasForeignKey(allergy => allergy.ClinicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(clinicalRecord => clinicalRecord.Notes)
                .WithOne(note => note.ClinicalRecord)
                .HasForeignKey(note => note.ClinicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(clinicalRecord => clinicalRecord.Diagnoses)
                .WithOne(diagnosis => diagnosis.ClinicalRecord)
                .HasForeignKey(diagnosis => diagnosis.ClinicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(clinicalRecord => clinicalRecord.SnapshotHistory)
                .WithOne(entry => entry.ClinicalRecord)
                .HasForeignKey(entry => entry.ClinicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(clinicalRecord => clinicalRecord.MedicalAnswers)
                .WithOne(answer => answer.ClinicalRecord)
                .HasForeignKey(answer => answer.ClinicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
