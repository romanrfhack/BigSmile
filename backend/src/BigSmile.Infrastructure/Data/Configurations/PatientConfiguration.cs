using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.HasKey(patient => patient.Id);

            builder.Property(patient => patient.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(patient => patient.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(patient => patient.DateOfBirth)
                .HasColumnType("date")
                .IsRequired();

            builder.Property(patient => patient.PrimaryPhone)
                .HasMaxLength(40);

            builder.Property(patient => patient.Email)
                .HasMaxLength(256);

            builder.Property(patient => patient.ResponsiblePartyName)
                .HasMaxLength(100);

            builder.Property(patient => patient.ResponsiblePartyRelationship)
                .HasMaxLength(100);

            builder.Property(patient => patient.ResponsiblePartyPhone)
                .HasMaxLength(40);

            builder.Property(patient => patient.IsActive)
                .IsRequired();

            builder.Property(patient => patient.CreatedAt)
                .IsRequired();

            builder.Property(patient => patient.UpdatedAt);

            builder.HasIndex(patient => new { patient.TenantId, patient.LastName, patient.FirstName });
            builder.HasIndex(patient => new { patient.TenantId, patient.PrimaryPhone });
            builder.HasIndex(patient => new { patient.TenantId, patient.Email });

            builder.HasOne(patient => patient.Tenant)
                .WithMany()
                .HasForeignKey(patient => patient.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
