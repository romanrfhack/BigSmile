using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderTemplateUsageTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReminderTemplateId",
                table: "AppointmentReminderLogEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReminderTemplateNameSnapshot",
                table: "AppointmentReminderLogEntries",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminderLogEntries_ReminderTemplateId",
                table: "AppointmentReminderLogEntries",
                column: "ReminderTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReminderLogEntries_ReminderTemplates_ReminderTemplateId",
                table: "AppointmentReminderLogEntries",
                column: "ReminderTemplateId",
                principalTable: "ReminderTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentReminderLogEntries_ReminderTemplates_ReminderTemplateId",
                table: "AppointmentReminderLogEntries");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentReminderLogEntries_ReminderTemplateId",
                table: "AppointmentReminderLogEntries");

            migrationBuilder.DropColumn(
                name: "ReminderTemplateId",
                table: "AppointmentReminderLogEntries");

            migrationBuilder.DropColumn(
                name: "ReminderTemplateNameSnapshot",
                table: "AppointmentReminderLogEntries");
        }
    }
}
