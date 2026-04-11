using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.HasKey(appointment => appointment.Id);

            builder.Property(appointment => appointment.StartsAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(appointment => appointment.EndsAt)
                .HasColumnType("datetime2")
                .IsRequired();

            builder.Property(appointment => appointment.Notes)
                .HasMaxLength(1000);

            builder.Property(appointment => appointment.CancellationReason)
                .HasMaxLength(500);

            builder.Property(appointment => appointment.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(appointment => appointment.CreatedAt)
                .IsRequired();

            builder.Property(appointment => appointment.UpdatedAt);

            builder.Property(appointment => appointment.CancelledAt);

            builder.HasIndex(appointment => new { appointment.TenantId, appointment.BranchId, appointment.StartsAt });
            builder.HasIndex(appointment => new { appointment.TenantId, appointment.PatientId, appointment.StartsAt });

            builder.HasOne(appointment => appointment.Tenant)
                .WithMany()
                .HasForeignKey(appointment => appointment.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(appointment => appointment.Branch)
                .WithMany()
                .HasForeignKey(appointment => appointment.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(appointment => appointment.Patient)
                .WithMany()
                .HasForeignKey(appointment => appointment.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
