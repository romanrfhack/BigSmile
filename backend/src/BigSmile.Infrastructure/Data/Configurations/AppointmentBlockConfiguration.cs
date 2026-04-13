using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class AppointmentBlockConfiguration : IEntityTypeConfiguration<AppointmentBlock>
    {
        public void Configure(EntityTypeBuilder<AppointmentBlock> builder)
        {
            builder.HasKey(block => block.Id);

            builder.Property(block => block.StartsAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(block => block.EndsAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(block => block.Label)
                .HasMaxLength(200);

            builder.Property(block => block.CreatedAt)
                .IsRequired();

            builder.HasIndex(block => new { block.TenantId, block.BranchId, block.StartsAt });

            builder.HasOne(block => block.Tenant)
                .WithMany()
                .HasForeignKey(block => block.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(block => block.Branch)
                .WithMany()
                .HasForeignKey(block => block.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
