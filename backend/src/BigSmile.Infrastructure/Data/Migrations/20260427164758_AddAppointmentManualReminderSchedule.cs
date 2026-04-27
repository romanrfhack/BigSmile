using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentManualReminderSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReminderChannel",
                table: "Appointments",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderCompletedAtUtc",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReminderCompletedByUserId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderDueAtUtc",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderRequired",
                table: "Appointments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderUpdatedAtUtc",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReminderUpdatedByUserId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_TenantId_BranchId_ReminderRequired_ReminderDueAtUtc",
                table: "Appointments",
                columns: new[] { "TenantId", "BranchId", "ReminderRequired", "ReminderDueAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Appointments_TenantId_BranchId_ReminderRequired_ReminderDueAtUtc",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderChannel",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderCompletedAtUtc",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderCompletedByUserId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderDueAtUtc",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderRequired",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderUpdatedAtUtc",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ReminderUpdatedByUserId",
                table: "Appointments");
        }
    }
}
