using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class BillingDocumentConfiguration : IEntityTypeConfiguration<BillingDocument>
    {
        public void Configure(EntityTypeBuilder<BillingDocument> builder)
        {
            builder.ToTable("BillingDocuments");

            builder.HasKey(billingDocument => billingDocument.Id);

            builder.Property(billingDocument => billingDocument.Status)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(billingDocument => billingDocument.CurrencyCode)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(billingDocument => billingDocument.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(billingDocument => billingDocument.CreatedAtUtc)
                .IsRequired();

            builder.Property(billingDocument => billingDocument.CreatedByUserId)
                .IsRequired();

            builder.Property(billingDocument => billingDocument.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(billingDocument => billingDocument.LastUpdatedByUserId)
                .IsRequired();

            builder.HasIndex(billingDocument => billingDocument.TreatmentQuoteId)
                .IsUnique();

            builder.HasIndex(billingDocument => billingDocument.PatientId);

            builder.HasOne(billingDocument => billingDocument.Tenant)
                .WithMany()
                .HasForeignKey(billingDocument => billingDocument.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(billingDocument => billingDocument.Patient)
                .WithMany()
                .HasForeignKey(billingDocument => billingDocument.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(billingDocument => billingDocument.TreatmentQuote)
                .WithMany()
                .HasForeignKey(billingDocument => billingDocument.TreatmentQuoteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(billingDocument => billingDocument.Items)
                .WithOne(item => item.BillingDocument)
                .HasForeignKey(item => item.BillingDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
