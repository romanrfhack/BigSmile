using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientDocumentConfiguration : IEntityTypeConfiguration<PatientDocument>
    {
        public void Configure(EntityTypeBuilder<PatientDocument> builder)
        {
            builder.ToTable("PatientDocuments");

            builder.HasKey(patientDocument => patientDocument.Id);

            builder.Property(patientDocument => patientDocument.OriginalFileName)
                .HasMaxLength(PatientDocument.OriginalFileNameMaxLength)
                .IsRequired();

            builder.Property(patientDocument => patientDocument.ContentType)
                .HasMaxLength(PatientDocument.ContentTypeMaxLength)
                .IsRequired();

            builder.Property(patientDocument => patientDocument.SizeBytes)
                .IsRequired();

            builder.Property(patientDocument => patientDocument.StorageKey)
                .HasMaxLength(PatientDocument.StorageKeyMaxLength)
                .IsRequired();

            builder.Property(patientDocument => patientDocument.UploadedAtUtc)
                .IsRequired();

            builder.Property(patientDocument => patientDocument.UploadedByUserId)
                .IsRequired();

            builder.Property(patientDocument => patientDocument.DeletedAtUtc);

            builder.Property(patientDocument => patientDocument.DeletedByUserId);

            builder.HasIndex(patientDocument => new { patientDocument.TenantId, patientDocument.PatientId });
            builder.HasIndex(patientDocument => patientDocument.PatientId);
            builder.HasIndex(patientDocument => patientDocument.StorageKey)
                .IsUnique();

            builder.HasOne(patientDocument => patientDocument.Tenant)
                .WithMany()
                .HasForeignKey(patientDocument => patientDocument.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(patientDocument => patientDocument.Patient)
                .WithMany()
                .HasForeignKey(patientDocument => patientDocument.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
