using BigSmile.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BigSmile.Infrastructure.Data.Configurations
{
    internal sealed class PatientPortalAccountConfiguration : IEntityTypeConfiguration<PatientPortalAccount>
    {
        public void Configure(EntityTypeBuilder<PatientPortalAccount> builder)
        {
            builder.ToTable("PatientPortalAccounts");

            builder.HasKey(account => account.Id);

            builder.Property(account => account.LoginName)
                .HasMaxLength(PatientPortalAccount.LoginNameMaxLength)
                .IsRequired();

            builder.Property(account => account.NormalizedLoginName)
                .HasMaxLength(PatientPortalAccount.LoginNameMaxLength)
                .IsRequired();

            builder.Property(account => account.PasswordHash)
                .HasMaxLength(PatientPortalAccount.PasswordHashMaxLength)
                .IsRequired();

            builder.Property(account => account.IsActive)
                .IsRequired();

            builder.Property(account => account.FailedLoginAttempts)
                .IsRequired();

            builder.Property(account => account.LockoutEndUtc);
            builder.Property(account => account.LastFailedLoginAtUtc);
            builder.Property(account => account.LastSuccessfulLoginAtUtc);

            builder.Property(account => account.SessionVersion)
                .IsRequired();

            builder.Property(account => account.CreatedAtUtc)
                .IsRequired();

            builder.Property(account => account.LastUpdatedAtUtc)
                .IsRequired();

            builder.Property(account => account.RowVersion)
                .IsRowVersion();

            builder.HasIndex(account => new { account.TenantId, account.NormalizedLoginName })
                .IsUnique();

            builder.HasIndex(account => new { account.TenantId, account.PatientId })
                .IsUnique()
                .HasFilter("[PatientId] IS NOT NULL");

            builder.HasIndex(account => account.PatientId);

            builder.HasOne(account => account.Tenant)
                .WithMany()
                .HasForeignKey(account => account.TenantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(account => account.Patient)
                .WithMany()
                .HasForeignKey(account => account.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
