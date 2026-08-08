using BigSmile.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260806131500_AddPatientIntakeSubmissionBoundary")]
    public sealed class AddPatientIntakeSubmissionBoundary : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAtUtc",
                table: "PatientIntakes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PatientIntakes_SubmissionMetadata",
                table: "PatientIntakes",
                sql: "(([Status] = N'Submitted' AND [SubmittedAtUtc] IS NOT NULL) OR ([Status] <> N'Submitted' AND [SubmittedAtUtc] IS NULL))");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_TenantId_PatientId_Status",
                table: "PatientIntakes",
                columns: new[] { "TenantId", "PatientId", "Status" },
                unique: true,
                filter: "[Status] = N'Submitted' AND [PatientId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PatientIntakes_TenantId_PatientId_Status",
                table: "PatientIntakes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PatientIntakes_SubmissionMetadata",
                table: "PatientIntakes");

            migrationBuilder.DropColumn(
                name: "SubmittedAtUtc",
                table: "PatientIntakes");
        }
    }
}
