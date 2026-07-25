using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientIntakeAuthenticationAuditEntryConfiguration
        : IEntityTypeConfiguration<PatientIntakeAuthenticationAuditEntry>
    {
        public void Configure(
            EntityTypeBuilder<PatientIntakeAuthenticationAuditEntry> builder)
        {
            builder.ToTable("PatientIntakeAuthenticationAuditEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.Action)
                .HasConversion<string>()
                .HasMaxLength(PatientIntakeAuthenticationAuditEntry.ActionMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorType)
                .HasConversion<string>()
                .HasMaxLength(PatientIntakeAuthenticationAuditEntry.ActorTypeMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorId)
                .IsRequired();
            builder.Property(entry => entry.OccurredAtUtc)
                .IsRequired();
            builder.Property(entry => entry.CorrelationId)
                .HasMaxLength(PatientIntakeAuthenticationAuditEntry.CorrelationIdMaxLength)
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
                entry.PatientIntakeId,
                entry.OccurredAtUtc
            });

            builder.HasIndex(entry => entry.PatientIntakeAccessLinkId);

            builder.HasOne(entry => entry.Tenant)
                .WithMany()
                .HasForeignKey(entry => entry.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientPortalAccount)
                .WithMany()
                .HasForeignKey(entry => entry.PatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientIntake)
                .WithMany()
                .HasForeignKey(entry => entry.PatientIntakeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientIntakeAccessLink)
                .WithMany()
                .HasForeignKey(entry => entry.PatientIntakeAccessLinkId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
