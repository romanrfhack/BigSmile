using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientIntakeRevisionConfiguration
        : IEntityTypeConfiguration<PatientIntakeRevision>
    {
        public void Configure(EntityTypeBuilder<PatientIntakeRevision> builder)
        {
            builder.ToTable("PatientIntakeRevisions", tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PatientIntakeRevisions_RevisionNumber",
                    "[RevisionNumber] > 0");
            });

            builder.HasKey(revision => revision.Id);

            builder.Property(revision => revision.RevisionNumber)
                .IsRequired();

            builder.Property(revision => revision.OccurredAtUtc)
                .IsRequired();

            builder.Property(revision => revision.ChangedFieldsJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(revision => revision.SnapshotJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(revision => revision.CorrelationId)
                .HasMaxLength(PatientIntakeRevision.CorrelationIdMaxLength)
                .IsRequired();

            builder.HasIndex(revision => new
                {
                    revision.TenantId,
                    revision.PatientIntakeId,
                    revision.RevisionNumber
                })
                .IsUnique();

            builder.HasIndex(revision => new
                {
                    revision.TenantId,
                    revision.ActorPatientPortalAccountId,
                    revision.OccurredAtUtc
                });

            builder.HasOne(revision => revision.Tenant)
                .WithMany()
                .HasForeignKey(revision => revision.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(revision => revision.ActorPatientPortalAccount)
                .WithMany()
                .HasForeignKey(revision => revision.ActorPatientPortalAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
