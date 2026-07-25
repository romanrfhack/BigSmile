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
            builder.ToTable("PatientIntakeAccessLinks", table =>
            {
                table.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_ExpiryOrder",
                    "[ExpiresAtUtc] > [CreatedAtUtc]");
                table.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_RevocationMetadata",
                    "(([RevokedAtUtc] IS NULL AND [RevokedByUserId] IS NULL) OR " +
                    "([RevokedAtUtc] IS NOT NULL AND [RevokedByUserId] IS NOT NULL))");
                table.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_ConsumptionMetadata",
                    "(([ConsumedAtUtc] IS NULL AND [ConsumedByPatientPortalAccountId] IS NULL) OR " +
                    "([ConsumedAtUtc] IS NOT NULL AND [ConsumedByPatientPortalAccountId] IS NOT NULL))");
                table.HasCheckConstraint(
                    "CK_PatientIntakeAccessLinks_SingleResolution",
                    "NOT ([RevokedAtUtc] IS NOT NULL AND [ConsumedAtUtc] IS NOT NULL)");
            });

            builder.HasKey(link => link.Id);

            builder.Property(link => link.Purpose)
                .HasConversion<string>()
                .HasMaxLength(PatientIntakeAccessLink.PurposeMaxLength)
                .IsRequired();

            builder.Property(link => link.TokenHash)
                .HasMaxLength(PatientIntakeAccessLink.TokenHashMaxLength)
                .IsRequired();

            builder.Property(link => link.CreatedAtUtc)
                .IsRequired();
            builder.Property(link => link.CreatedByUserId)
                .IsRequired();
            builder.Property(link => link.ExpiresAtUtc)
                .IsRequired();
            builder.Property(link => link.RevokedAtUtc);
            builder.Property(link => link.RevokedByUserId);
            builder.Property(link => link.ConsumedAtUtc);
            builder.Property(link => link.ConsumedByPatientPortalAccountId);

            builder.Property(link => link.RowVersion)
                .IsRowVersion();

            builder.HasIndex(link => link.TokenHash)
                .IsUnique();

            builder.HasIndex(link => new
            {
                link.TenantId,
                link.ExpiresAtUtc
            });

            builder.HasIndex(link => new
            {
                link.TenantId,
                link.BranchId,
                link.CreatedAtUtc
            });

            builder.HasIndex(link => new
            {
                link.TenantId,
                link.CreatedAtUtc
            });

            builder.HasOne(link => link.Tenant)
                .WithMany()
                .HasForeignKey(link => link.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(link => link.Branch)
                .WithMany()
                .HasForeignKey(link => link.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(link => link.ConsumedByPatientPortalAccount)
                .WithMany()
                .HasForeignKey(link => link.ConsumedByPatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
