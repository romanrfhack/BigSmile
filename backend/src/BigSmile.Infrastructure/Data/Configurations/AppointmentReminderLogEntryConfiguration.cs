using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class AppointmentReminderLogEntryConfiguration : IEntityTypeConfiguration<AppointmentReminderLogEntry>
    {
        public void Configure(EntityTypeBuilder<AppointmentReminderLogEntry> builder)
        {
            builder.ToTable("AppointmentReminderLogEntries");

            builder.HasKey(entry => entry.Id);

            builder.Property(entry => entry.Channel)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(entry => entry.Outcome)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(entry => entry.Notes)
                .HasMaxLength(AppointmentReminderLogEntry.NotesMaxLength);

            builder.Property(entry => entry.ReminderTemplateNameSnapshot)
                .HasMaxLength(ReminderTemplate.NameMaxLength);

            builder.Property(entry => entry.CreatedAtUtc)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(entry => entry.CreatedByUserId)
                .IsRequired();

            builder.HasIndex(entry => new { entry.TenantId, entry.AppointmentId, entry.CreatedAtUtc });
            builder.HasIndex(entry => new { entry.AppointmentId, entry.CreatedAtUtc });

            builder.HasOne(entry => entry.Tenant)
                .WithMany()
                .HasForeignKey(entry => entry.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(entry => entry.Appointment)
                .WithMany(appointment => appointment.ReminderLogEntries)
                .HasForeignKey(entry => entry.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(entry => entry.ReminderTemplate)
                .WithMany()
                .HasForeignKey(entry => entry.ReminderTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
