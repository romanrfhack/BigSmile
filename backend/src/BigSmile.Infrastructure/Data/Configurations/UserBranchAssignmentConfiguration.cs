using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal class UserBranchAssignmentConfiguration : IEntityTypeConfiguration<UserBranchAssignment>
    {
        public void Configure(EntityTypeBuilder<UserBranchAssignment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasIndex(a => new { a.MembershipId, a.BranchId })
                .IsUnique();

            builder.Property(a => a.IsActive)
                .IsRequired();

            builder.Property(a => a.AssignedAt)
                .IsRequired();

            builder.Property(a => a.UnassignedAt);

            // Relationships
            builder.HasOne(a => a.Membership)
                .WithMany(m => m.BranchAssignments)
                .HasForeignKey(a => a.MembershipId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(a => a.Branch)
                .WithMany() // Branch does not have a navigation property to assignments yet
                .HasForeignKey(a => a.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}