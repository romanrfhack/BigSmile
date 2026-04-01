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

            // Seed system roles
            builder.HasData(
                new Role(
                    id: Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    name: "PlatformAdmin",
                    description: "Platform‑level administrator with full access across all tenants.",
                    isSystem: true),
                new Role(
                    id: Guid.Parse("00000000-0000-0000-0000-000000000002"),
                    name: "TenantAdmin",
                    description: "Tenant administrator with full access within a single tenant.",
                    isSystem: true),
                new Role(
                    id: Guid.Parse("00000000-0000-0000-0000-000000000003"),
                    name: "TenantUser",
                    description: "Regular user within a tenant.",
                    isSystem: true)
            );
        }
    }
}