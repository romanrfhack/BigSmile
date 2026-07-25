using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientIntakeAccessLinkAuditEntryConfiguration
        : IEntityTypeConfiguration<PatientIntakeAccessLinkAuditEntry>
    {
        public void Configure(
            EntityTypeBuilder<PatientIntakeAccessLinkAuditEntry> builder)
        {
            builder.ToTable("PatientIntakeAccessLinkAuditEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.Action)
                .HasConversion<string>()
                .HasMaxLength(PatientIntakeAccessLinkAuditEntry.ActionMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorType)
                .HasConversion<string>()
                .HasMaxLength(PatientIntakeAccessLinkAuditEntry.ActorTypeMaxLength)
                .IsRequired();

            builder.Property(entry => entry.ActorId)
                .IsRequired();
            builder.Property(entry => entry.OccurredAtUtc)
                .IsRequired();
            builder.Property(entry => entry.CorrelationId)
                .HasMaxLength(PatientIntakeAccessLinkAuditEntry.CorrelationIdMaxLength)
                .IsRequired();

            builder.HasIndex(entry => new
            {
                entry.TenantId,
                entry.PatientIntakeAccessLinkId,
                entry.OccurredAtUtc
            });

            builder.HasIndex(entry => new
            {
                entry.TenantId,
                entry.ActorId,
                entry.OccurredAtUtc
            });

            builder.HasOne(entry => entry.Tenant)
                .WithMany()
                .HasForeignKey(entry => entry.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.Branch)
                .WithMany()
                .HasForeignKey(entry => entry.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.PatientIntakeAccessLink)
                .WithMany()
                .HasForeignKey(entry => entry.PatientIntakeAccessLinkId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
