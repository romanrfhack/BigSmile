using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class ReminderTemplateConfiguration : IEntityTypeConfiguration<ReminderTemplate>
    {
        public void Configure(EntityTypeBuilder<ReminderTemplate> builder)
        {
            builder.ToTable("ReminderTemplates");

            builder.HasKey(template => template.Id);

            builder.Property(template => template.Name)
                .HasMaxLength(ReminderTemplate.NameMaxLength)
                .IsRequired();

            builder.Property(template => template.Body)
                .HasMaxLength(ReminderTemplate.BodyMaxLength)
                .IsRequired();

            builder.Property(template => template.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(template => template.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(template => template.CreatedByUserId)
                .IsRequired();

            builder.Property(template => template.UpdatedAtUtc)
                .HasColumnType("datetime2");

            builder.Property(template => template.UpdatedByUserId);

            builder.Property(template => template.DeactivatedAtUtc)
                .HasColumnType("datetime2");

            builder.Property(template => template.DeactivatedByUserId);

            builder.HasIndex(template => new { template.TenantId, template.IsActive, template.Name });

            builder.HasOne(template => template.Tenant)
                .WithMany()
                .HasForeignKey(template => template.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
