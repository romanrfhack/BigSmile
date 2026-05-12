using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalEncounterVitals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalEncounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConsultationType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TemperatureC = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: true),
                    BloodPressureSystolic = table.Column<int>(type: "int", nullable: true),
                    BloodPressureDiastolic = table.Column<int>(type: "int", nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    RespiratoryRatePerMinute = table.Column<int>(type: "int", nullable: true),
                    HeartRateBpm = table.Column<int>(type: "int", nullable: true),
                    ClinicalNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalEncounters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalEncounters_ClinicalNotes_ClinicalNoteId",
                        column: x => x.ClinicalNoteId,
                        principalTable: "ClinicalNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalEncounters_ClinicalRecords_ClinicalRecordId",
                        column: x => x.ClinicalRecordId,
                        principalTable: "ClinicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicalEncounters_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalEncounters_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEncounters_ClinicalNoteId",
                table: "ClinicalEncounters",
                column: "ClinicalNoteId",
                unique: true,
                filter: "[ClinicalNoteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEncounters_ClinicalRecordId",
                table: "ClinicalEncounters",
                column: "ClinicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEncounters_PatientId",
                table: "ClinicalEncounters",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEncounters_TenantId_ClinicalRecordId_OccurredAtUtc",
                table: "ClinicalEncounters",
                columns: new[] { "TenantId", "ClinicalRecordId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalEncounters_TenantId_PatientId_OccurredAtUtc",
                table: "ClinicalEncounters",
                columns: new[] { "TenantId", "PatientId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalEncounters");
        }
    }
}
