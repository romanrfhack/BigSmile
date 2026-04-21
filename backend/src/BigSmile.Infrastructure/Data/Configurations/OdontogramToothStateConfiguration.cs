using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class OdontogramToothStateConfiguration : IEntityTypeConfiguration<OdontogramToothState>
    {
        public void Configure(EntityTypeBuilder<OdontogramToothState> builder)
        {
            builder.ToTable("OdontogramToothStates");

            builder.HasKey(tooth => tooth.Id);

            builder.Property(tooth => tooth.ToothCode)
                .IsRequired()
                .HasMaxLength(2);

            builder.Property(tooth => tooth.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(tooth => tooth.UpdatedAtUtc)
                .IsRequired();

            builder.Property(tooth => tooth.UpdatedByUserId)
                .IsRequired();

            builder.HasIndex(tooth => new { tooth.OdontogramId, tooth.ToothCode })
                .IsUnique();
        }
    }
}
