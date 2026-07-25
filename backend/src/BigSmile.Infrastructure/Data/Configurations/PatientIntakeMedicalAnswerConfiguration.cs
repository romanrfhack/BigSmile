using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientIntakeMedicalAnswerConfiguration
        : IEntityTypeConfiguration<PatientIntakeMedicalAnswer>
    {
        public void Configure(EntityTypeBuilder<PatientIntakeMedicalAnswer> builder)
        {
            builder.ToTable("PatientIntakeMedicalAnswers");

            builder.HasKey(answer => answer.Id);

            builder.Property(answer => answer.QuestionKey)
                .HasMaxLength(ClinicalMedicalQuestionnaireCatalog.QuestionKeyMaxLength)
                .IsRequired();

            builder.Property(answer => answer.Answer)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(answer => answer.Details)
                .HasMaxLength(ClinicalMedicalAnswer.DetailsMaxLength);

            builder.Property(answer => answer.LastUpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(answer => new
                {
                    answer.TenantId,
                    answer.PatientIntakeId,
                    answer.QuestionKey
                })
                .IsUnique();

            builder.HasOne(answer => answer.Tenant)
                .WithMany()
                .HasForeignKey(answer => answer.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
