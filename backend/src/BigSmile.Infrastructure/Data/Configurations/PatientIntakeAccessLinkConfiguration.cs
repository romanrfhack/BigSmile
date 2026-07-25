using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientIntakeAccessLinkConfiguration
        : IEntityTypeConfiguration<PatientIntakeAccessLink>
    {
        public void Configure(EntityTypeBuilder<PatientIntakeAccessLink> builder)
        {
            builder.ToTable("PatientIntakeAccessLinks", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_ExpiryOrder",
                    "[ExpiresAtUtc] > [CreatedAtUtc]");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_RevocationState",
                    "(([RevokedAtUtc] IS NULL AND [RevokedByUserId] IS NULL) OR " +
                    "([RevokedAtUtc] IS NOT NULL AND [RevokedByUserId] IS NOT NULL))");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_ConsumptionState",
                    "(([ConsumedAtUtc] IS NULL AND [ConsumedByPatientPortalAccountId] IS NULL AND [PatientIntakeId] IS NULL) OR " +
                    "([ConsumedAtUtc] IS NOT NULL AND [ConsumedByPatientPortalAccountId] IS NOT NULL AND [PatientIntakeId] IS NOT NULL))");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_TerminalState",
                    "NOT ([RevokedAtUtc] IS NOT NULL AND [ConsumedAtUtc] IS NOT NULL)");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_RevokedAtWindow",
                    "[RevokedAtUtc] IS NULL OR ([RevokedAtUtc] >= [CreatedAtUtc] AND [RevokedAtUtc] < [ExpiresAtUtc])");
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_ConsumedAtWindow",
                    "[ConsumedAtUtc] IS NULL OR ([ConsumedAtUtc] >= [CreatedAtUtc] AND [ConsumedAtUtc] < [ExpiresAtUtc])");
            });

            builder.HasKey(accessLink => accessLink.Id);

            builder.Property(accessLink => accessLink.TokenHash)
                .HasMaxLength(PatientIntakeAccessLink.TokenHashMaxLength)
                .IsRequired();

            builder.Property(accessLink => accessLink.CreatedAtUtc)
                .IsRequired();

            builder.Property(accessLink => accessLink.CreatedByUserId)
                .IsRequired();

            builder.Property(accessLink => accessLink.ExpiresAtUtc)
                .IsRequired();

            builder.Property(accessLink => accessLink.RowVersion)
                .IsRowVersion();

            builder.HasIndex(accessLink => accessLink.TokenHash)
                .IsUnique();

            builder.HasIndex(accessLink => new
                {
                    accessLink.TenantId,
                    accessLink.CreatedAtUtc
                });

            builder.HasIndex(accessLink => new
                {
                    accessLink.TenantId,
                    accessLink.ExpiresAtUtc
                });

            builder.HasIndex(accessLink => new
                {
                    accessLink.TenantId,
                    accessLink.BranchId,
                    accessLink.CreatedAtUtc
                });

            builder.HasIndex(accessLink => accessLink.ConsumedByPatientPortalAccountId)
                .IsUnique()
                .HasFilter("[ConsumedByPatientPortalAccountId] IS NOT NULL");

            builder.HasIndex(accessLink => accessLink.PatientIntakeId)
                .IsUnique()
                .HasFilter("[PatientIntakeId] IS NOT NULL");

            builder.HasOne(accessLink => accessLink.Tenant)
                .WithMany()
                .HasForeignKey(accessLink => accessLink.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(accessLink => accessLink.Branch)
                .WithMany()
                .HasForeignKey(accessLink => accessLink.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(accessLink => accessLink.ConsumedByPatientPortalAccount)
                .WithMany()
                .HasForeignKey(accessLink => accessLink.ConsumedByPatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(accessLink => accessLink.PatientIntake)
                .WithMany()
                .HasForeignKey(accessLink => accessLink.PatientIntakeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
