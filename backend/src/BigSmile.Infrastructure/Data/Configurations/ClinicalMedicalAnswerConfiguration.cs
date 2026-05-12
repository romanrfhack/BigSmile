using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ClinicalMedicalAnswerConfiguration : IEntityTypeConfiguration<ClinicalMedicalAnswer>
    {
        public void Configure(EntityTypeBuilder<ClinicalMedicalAnswer> builder)
        {
            builder.ToTable("ClinicalMedicalAnswers");

            builder.HasKey(answer => answer.Id);

            builder.Property(answer => answer.QuestionKey)
                .IsRequired()
                .HasMaxLength(ClinicalMedicalQuestionnaireCatalog.QuestionKeyMaxLength);

            builder.Property(answer => answer.Answer)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(answer => answer.Details)
                .HasMaxLength(ClinicalMedicalAnswer.DetailsMaxLength);

            builder.Property(answer => answer.UpdatedAtUtc)
                .IsRequired();

            builder.Property(answer => answer.UpdatedByUserId)
                .IsRequired();

            builder.HasIndex(answer => new { answer.TenantId, answer.ClinicalRecordId, answer.QuestionKey })
                .IsUnique();

            builder.HasIndex(answer => new { answer.TenantId, answer.PatientId });

            builder.HasOne(answer => answer.Tenant)
                .WithMany()
                .HasForeignKey(answer => answer.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(answer => answer.Patient)
                .WithMany()
                .HasForeignKey(answer => answer.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
