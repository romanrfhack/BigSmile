using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.NormalizedEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique();

            builder.Property(u => u.HashedPassword)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(u => u.DisplayName)
                .HasMaxLength(200);

            builder.Property(u => u.IsActive)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt);

            // One-to-many: User -> UserTenantMemberships
            builder.HasMany(u => u.TenantMemberships)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}