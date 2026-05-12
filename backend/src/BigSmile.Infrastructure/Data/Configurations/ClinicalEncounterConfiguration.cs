using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalEncounterConfiguration : IEntityTypeConfiguration<ClinicalEncounter>
    {
        public void Configure(EntityTypeBuilder<ClinicalEncounter> builder)
        {
            builder.ToTable("ClinicalEncounters");

            builder.HasKey(encounter => encounter.Id);

            builder.Property(encounter => encounter.OccurredAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(encounter => encounter.ChiefComplaint)
                .IsRequired()
                .HasMaxLength(ClinicalEncounter.ChiefComplaintMaxLength);

            builder.Property(encounter => encounter.ConsultationType)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(encounter => encounter.TemperatureC)
                .HasPrecision(4, 1);

            builder.Property(encounter => encounter.WeightKg)
                .HasPrecision(6, 2);

            builder.Property(encounter => encounter.HeightCm)
                .HasPrecision(5, 2);

            builder.Property(encounter => encounter.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(encounter => encounter.CreatedByUserId)
                .IsRequired();

            builder.HasIndex(encounter => new { encounter.TenantId, encounter.PatientId, encounter.OccurredAtUtc });

            builder.HasIndex(encounter => new { encounter.TenantId, encounter.ClinicalRecordId, encounter.OccurredAtUtc });

            builder.HasIndex(encounter => encounter.ClinicalNoteId)
                .IsUnique()
                .HasFilter("[ClinicalNoteId] IS NOT NULL");

            builder.HasOne(encounter => encounter.Tenant)
                .WithMany()
                .HasForeignKey(encounter => encounter.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(encounter => encounter.Patient)
                .WithMany()
                .HasForeignKey(encounter => encounter.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(encounter => encounter.ClinicalNote)
                .WithMany()
                .HasForeignKey(encounter => encounter.ClinicalNoteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
