using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalRecordFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalBackgroundSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CurrentMedicationsSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalRecords_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalNotes_ClinicalRecords_ClinicalRecordId",
                        column: x => x.ClinicalRecordId,
                        principalTable: "ClinicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalRecordAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Substance = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReactionSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalRecordAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalRecordAllergies_ClinicalRecords_ClinicalRecordId",
                        column: x => x.ClinicalRecordId,
                        principalTable: "ClinicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_ClinicalRecordId_CreatedAtUtc",
                table: "ClinicalNotes",
                columns: new[] { "ClinicalRecordId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalRecordAllergies_ClinicalRecordId_Substance",
                table: "ClinicalRecordAllergies",
                columns: new[] { "ClinicalRecordId", "Substance" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalRecords_PatientId",
                table: "ClinicalRecords",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalRecords_TenantId_PatientId",
                table: "ClinicalRecords",
                columns: new[] { "TenantId", "PatientId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalNotes");

            migrationBuilder.DropTable(
                name: "ClinicalRecordAllergies");

            migrationBuilder.DropTable(
                name: "ClinicalRecords");
        }
    }
}
