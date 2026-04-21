using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class TreatmentPlanItemConfiguration : IEntityTypeConfiguration<TreatmentPlanItem>
    {
        public void Configure(EntityTypeBuilder<TreatmentPlanItem> builder)
        {
            builder.ToTable("TreatmentPlanItems");

            builder.HasKey(item => item.Id);

            builder.Property(item => item.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(item => item.Category)
                .HasMaxLength(100);

            builder.Property(item => item.Notes)
                .HasMaxLength(500);

            builder.Property(item => item.ToothCode)
                .HasMaxLength(2);

            builder.Property(item => item.SurfaceCode)
                .HasMaxLength(1);

            builder.Property(item => item.Quantity)
                .IsRequired();

            builder.Property(item => item.CreatedAtUtc)
                .IsRequired();

            builder.Property(item => item.CreatedByUserId)
                .IsRequired();

            builder.HasIndex(item => item.TreatmentPlanId);
        }
    }
}
