using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientPortalSecurityAuditEntryConfiguration : IEntityTypeConfiguration<PatientPortalSecurityAuditEntry>
    {
        public void Configure(EntityTypeBuilder<PatientPortalSecurityAuditEntry> builder)
        {
            builder.ToTable("PatientPortalSecurityAuditEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.Action)
                .HasConversion<string>()
                .HasMaxLength(PatientPortalSecurityAuditEntry.ActionMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorUserId)
                .IsRequired();

            builder.Property(entry => entry.OccurredAtUtc)
                .IsRequired();

            builder.Property(entry => entry.CorrelationId)
                .HasMaxLength(PatientPortalSecurityAuditEntry.CorrelationIdMaxLength)
                .IsRequired();

            builder.HasIndex(entry => new
                {
                    entry.TenantId,
                    entry.PatientId,
                    entry.OccurredAtUtc
                });

            builder.HasIndex(entry => new
                {
                    entry.TenantId,
                    entry.PatientPortalInvitationId,
                    entry.OccurredAtUtc
                });

            builder.HasOne(entry => entry.Tenant)
                .WithMany()
                .HasForeignKey(entry => entry.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(entry => entry.Patient)
                .WithMany()
                .HasForeignKey(entry => entry.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientPortalInvitation)
                .WithMany()
                .HasForeignKey(entry => entry.PatientPortalInvitationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
