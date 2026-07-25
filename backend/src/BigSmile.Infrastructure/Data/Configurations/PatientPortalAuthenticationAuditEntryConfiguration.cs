using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientPortalAuthenticationAuditEntryConfiguration
        : IEntityTypeConfiguration<PatientPortalAuthenticationAuditEntry>
    {
        public void Configure(EntityTypeBuilder<PatientPortalAuthenticationAuditEntry> builder)
        {
            builder.ToTable("PatientPortalAuthenticationAuditEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.Action)
                .HasConversion<string>()
                .HasMaxLength(PatientPortalAuthenticationAuditEntry.ActionMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorType)
                .HasConversion<string>()
                .HasMaxLength(PatientPortalAuthenticationAuditEntry.ActorTypeMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorId)
                .IsRequired();

            builder.Property(entry => entry.OccurredAtUtc)
                .IsRequired();

            builder.Property(entry => entry.CorrelationId)
                .HasMaxLength(PatientPortalAuthenticationAuditEntry.CorrelationIdMaxLength)
                .IsRequired();

            builder.HasIndex(entry => new
            {
                entry.TenantId,
                entry.PatientPortalAccountId,
                entry.OccurredAtUtc
            });

            builder.HasIndex(entry => new
            {
                entry.TenantId,
                entry.PatientId,
                entry.OccurredAtUtc
            });

            builder.HasIndex(entry => entry.PatientPortalInvitationId);

            builder.HasOne(entry => entry.Tenant)
                .WithMany()
                .HasForeignKey(entry => entry.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(entry => entry.Patient)
                .WithMany()
                .HasForeignKey(entry => entry.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientPortalAccount)
                .WithMany()
                .HasForeignKey(entry => entry.PatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientPortalInvitation)
                .WithMany()
                .HasForeignKey(entry => entry.PatientPortalInvitationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
