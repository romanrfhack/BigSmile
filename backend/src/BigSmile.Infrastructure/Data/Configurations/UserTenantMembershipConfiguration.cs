using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal class UserTenantMembershipConfiguration : IEntityTypeConfiguration<UserTenantMembership>
    {
        public void Configure(EntityTypeBuilder<UserTenantMembership> builder)
        {
            builder.HasKey(m => m.Id);

            builder.HasIndex(m => new { m.UserId, m.TenantId })
                .IsUnique();

            builder.Property(m => m.IsActive)
                .IsRequired();

            builder.Property(m => m.JoinedAt)
                .IsRequired();

            builder.Property(m => m.LeftAt);

            // Relationships
            builder.HasOne(m => m.User)
                .WithMany(u => u.TenantMemberships)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Tenant)
                .WithMany() // Tenant does not have a navigation property to memberships yet
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Role)
                .WithMany(r => r.UserTenantMemberships)
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // One-to-many: Membership -> BranchAssignments
            builder.HasMany(m => m.BranchAssignments)
                .WithOne(a => a.Membership)
                .HasForeignKey(a => a.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}