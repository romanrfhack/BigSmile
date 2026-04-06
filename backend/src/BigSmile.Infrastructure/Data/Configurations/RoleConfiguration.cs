using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(r => r.Name)
                .IsUnique();

            builder.Property(r => r.Description)
                .HasMaxLength(500);

            builder.Property(r => r.IsSystem)
                .IsRequired();

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            // One-to-many: Role -> UserTenantMemberships
            builder.HasMany(r => r.UserTenantMemberships)
                .WithOne(m => m.Role)
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // Do not cascade delete role

            // Seed system roles with deterministic values for migrations
            builder.HasData(
                new
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    Name = "PlatformAdmin",
                    Description = "Platform‑level administrator with full access across all tenants.",
                    IsSystem = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    Name = "TenantAdmin",
                    Description = "Tenant administrator with full access within a single tenant.",
                    IsSystem = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    Name = "TenantUser",
                    Description = "Regular user within a tenant.",
                    IsSystem = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}