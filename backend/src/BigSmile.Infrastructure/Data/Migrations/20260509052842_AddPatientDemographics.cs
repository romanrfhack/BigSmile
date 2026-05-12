using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientDemographics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MaritalStatus",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unspecified");

            migrationBuilder.AddColumn<string>(
                name: "Occupation",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferredBy",
                table: "Patients",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sex",
                table: "Patients",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Unspecified");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Occupation",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "ReferredBy",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Sex",
                table: "Patients");
        }
    }
}
