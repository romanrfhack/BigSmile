using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientIntakeConfiguration : IEntityTypeConfiguration<PatientIntake>
    {
        public void Configure(EntityTypeBuilder<PatientIntake> builder)
        {
            builder.ToTable("PatientIntakes", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakes_OriginPatientLink",
                    "(([Origin] = N'ExistingPatientPortal' AND [PatientId] IS NOT NULL AND [CanonicalPatientBaselineJson] IS NOT NULL AND [CanonicalPatientBaselineCapturedAtUtc] IS NOT NULL) OR " +
                    "([Origin] = N'NewPatientWaitingRoom' AND [PatientId] IS NULL AND [CanonicalPatientBaselineJson] IS NULL AND [CanonicalPatientBaselineCapturedAtUtc] IS NULL))");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakes_ExpiryOrder",
                    "[ExpiresAtUtc] > [CreatedAtUtc]");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakes_CurrentRevisionNumber",
                    "[CurrentRevisionNumber] >= 0");
            });

            builder.HasKey(intake => intake.Id);

            builder.Property(intake => intake.Origin)
                .HasConversion<string>()
                .HasMaxLength(40)
                .IsRequired();

            builder.Property(intake => intake.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(intake => intake.FirstName)
                .HasMaxLength(PatientIntake.NameMaxLength);

            builder.Property(intake => intake.LastName)
                .HasMaxLength(PatientIntake.NameMaxLength);

            builder.Property(intake => intake.Sex)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(intake => intake.Occupation)
                .HasMaxLength(PatientIntake.DemographicMaxLength);

            builder.Property(intake => intake.MaritalStatus)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(intake => intake.ReferredBy)
                .HasMaxLength(PatientIntake.DemographicMaxLength);

            builder.Property(intake => intake.PreferredPhone)
                .HasMaxLength(PatientIntake.PhoneMaxLength);

            builder.Property(intake => intake.MobilePhone)
                .HasMaxLength(PatientIntake.PhoneMaxLength);

            builder.Property(intake => intake.HomePhone)
                .HasMaxLength(PatientIntake.PhoneMaxLength);

            builder.Property(intake => intake.WorkPhone)
                .HasMaxLength(PatientIntake.PhoneMaxLength);

            builder.Property(intake => intake.Email)
                .HasMaxLength(PatientIntake.EmailMaxLength);

            builder.Property(intake => intake.ResponsiblePartyName)
                .HasMaxLength(PatientIntake.NameMaxLength);

            builder.Property(intake => intake.ResponsiblePartyRelationship)
                .HasMaxLength(PatientIntake.DemographicMaxLength);

            builder.Property(intake => intake.ResponsiblePartyPhone)
                .HasMaxLength(PatientIntake.PhoneMaxLength);

            builder.Property(intake => intake.ReasonForVisit)
                .HasMaxLength(PatientIntake.ReasonForVisitMaxLength);

            builder.Property(intake => intake.CanonicalPatientBaselineJson)
                .HasColumnType("nvarchar(max)");

            builder.Property(intake => intake.CurrentRevisionNumber)
                .IsRequired();

            builder.Property(intake => intake.CreatedAtUtc)
                .IsRequired();

            builder.Property(intake => intake.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(intake => intake.ExpiresAtUtc)
                .IsRequired();

            builder.Property(intake => intake.RowVersion)
                .IsRowVersion();

            builder.HasIndex(intake => new
                {
                    intake.TenantId,
                    intake.PatientPortalAccountId
                })
                .IsUnique()
                .HasFilter("[Status] = N'Draft'");

            builder.HasIndex(intake => new
                {
                    intake.TenantId,
                    intake.Status,
                    intake.ExpiresAtUtc
                });

            builder.HasIndex(intake => new
                {
                    intake.TenantId,
                    intake.PatientId
                });

            builder.HasIndex(intake => intake.BranchId);

            builder.HasOne(intake => intake.Tenant)
                .WithMany()
                .HasForeignKey(intake => intake.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(intake => intake.PatientPortalAccount)
                .WithMany()
                .HasForeignKey(intake => intake.PatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(intake => intake.Patient)
                .WithMany()
                .HasForeignKey(intake => intake.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(intake => intake.Branch)
                .WithMany()
                .HasForeignKey(intake => intake.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(intake => intake.MedicalAnswers)
                .WithOne(answer => answer.PatientIntake)
                .HasForeignKey(answer => answer.PatientIntakeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(intake => intake.Revisions)
                .WithOne(revision => revision.PatientIntake)
                .HasForeignKey(revision => revision.PatientIntakeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
