using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientPortalInvitationConfiguration : IEntityTypeConfiguration<PatientPortalInvitation>
    {
        public void Configure(EntityTypeBuilder<PatientPortalInvitation> builder)
        {
            builder.ToTable("PatientPortalInvitations");

            builder.HasKey(invitation => invitation.Id);

            builder.Property(invitation => invitation.Purpose)
                .HasConversion<string>()
                .HasMaxLength(PatientPortalInvitation.PurposeMaxLength)
                .IsRequired();

            builder.Property(invitation => invitation.TokenHash)
                .HasMaxLength(PatientPortalInvitation.TokenHashMaxLength)
                .IsRequired();

            builder.Property(invitation => invitation.CreatedAtUtc)
                .IsRequired();

            builder.Property(invitation => invitation.CreatedByUserId)
                .IsRequired();

            builder.Property(invitation => invitation.ExpiresAtUtc)
                .IsRequired();

            builder.Property(invitation => invitation.RevokedAtUtc);
            builder.Property(invitation => invitation.RevokedByUserId);
            builder.Property(invitation => invitation.ConsumedAtUtc);
            builder.Property(invitation => invitation.ConsumedByPatientPortalAccountId);

            builder.Property(invitation => invitation.RowVersion)
                .IsRowVersion();

            builder.HasIndex(invitation => invitation.TokenHash)
                .IsUnique();

            builder.HasIndex(invitation => new
                {
                    invitation.TenantId,
                    invitation.PatientId,
                    invitation.CreatedAtUtc
                });

            builder.HasIndex(invitation => new
                {
                    invitation.TenantId,
                    invitation.ExpiresAtUtc
                });

            builder.HasOne(invitation => invitation.Tenant)
                .WithMany()
                .HasForeignKey(invitation => invitation.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(invitation => invitation.Patient)
                .WithMany()
                .HasForeignKey(invitation => invitation.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(invitation => invitation.ConsumedByPatientPortalAccount)
                .WithMany()
                .HasForeignKey(invitation => invitation.ConsumedByPatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
