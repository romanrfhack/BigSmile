using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class TreatmentPlanConfiguration : IEntityTypeConfiguration<TreatmentPlan>
    {
        public void Configure(EntityTypeBuilder<TreatmentPlan> builder)
        {
            builder.ToTable("TreatmentPlans");

            builder.HasKey(treatmentPlan => treatmentPlan.Id);

            builder.Property(treatmentPlan => treatmentPlan.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(treatmentPlan => treatmentPlan.CreatedAtUtc)
                .IsRequired();

            builder.Property(treatmentPlan => treatmentPlan.CreatedByUserId)
                .IsRequired();

            builder.Property(treatmentPlan => treatmentPlan.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(treatmentPlan => treatmentPlan.LastUpdatedByUserId)
                .IsRequired();

            builder.HasIndex(treatmentPlan => new { treatmentPlan.TenantId, treatmentPlan.PatientId })
                .IsUnique();

            builder.HasOne(treatmentPlan => treatmentPlan.Tenant)
                .WithMany()
                .HasForeignKey(treatmentPlan => treatmentPlan.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(treatmentPlan => treatmentPlan.Patient)
                .WithMany()
                .HasForeignKey(treatmentPlan => treatmentPlan.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(treatmentPlan => treatmentPlan.Items)
                .WithOne(item => item.TreatmentPlan)
                .HasForeignKey(item => item.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
