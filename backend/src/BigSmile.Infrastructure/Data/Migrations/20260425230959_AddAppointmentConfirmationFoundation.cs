using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentConfirmationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationStatus",
                table: "Appointments",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedAtUtc",
                table: "Appointments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ConfirmedByUserId",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationStatus",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConfirmedAtUtc",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ConfirmedByUserId",
                table: "Appointments");
        }
    }
}
