using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class TreatmentQuoteConfiguration : IEntityTypeConfiguration<TreatmentQuote>
    {
        public void Configure(EntityTypeBuilder<TreatmentQuote> builder)
        {
            builder.ToTable("TreatmentQuotes");

            builder.HasKey(treatmentQuote => treatmentQuote.Id);

            builder.Property(treatmentQuote => treatmentQuote.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(treatmentQuote => treatmentQuote.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(treatmentQuote => treatmentQuote.CreatedAtUtc)
                .IsRequired();

            builder.Property(treatmentQuote => treatmentQuote.CreatedByUserId)
                .IsRequired();

            builder.Property(treatmentQuote => treatmentQuote.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(treatmentQuote => treatmentQuote.LastUpdatedByUserId)
                .IsRequired();

            builder.HasIndex(treatmentQuote => treatmentQuote.TreatmentPlanId)
                .IsUnique();

            builder.HasIndex(treatmentQuote => treatmentQuote.PatientId);

            builder.HasOne(treatmentQuote => treatmentQuote.Tenant)
                .WithMany()
                .HasForeignKey(treatmentQuote => treatmentQuote.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(treatmentQuote => treatmentQuote.Patient)
                .WithMany()
                .HasForeignKey(treatmentQuote => treatmentQuote.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(treatmentQuote => treatmentQuote.TreatmentPlan)
                .WithMany()
                .HasForeignKey(treatmentQuote => treatmentQuote.TreatmentPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(treatmentQuote => treatmentQuote.Items)
                .WithOne(item => item.TreatmentQuote)
                .HasForeignKey(item => item.TreatmentQuoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
