using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalMedicalQuestionnaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalMedicalAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClinicalRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicalMedicalAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalMedicalAnswers_ClinicalRecords_ClinicalRecordId",
                        column: x => x.ClinicalRecordId,
                        principalTable: "ClinicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClinicalMedicalAnswers_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClinicalMedicalAnswers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalMedicalAnswers_ClinicalRecordId",
                table: "ClinicalMedicalAnswers",
                column: "ClinicalRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalMedicalAnswers_PatientId",
                table: "ClinicalMedicalAnswers",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalMedicalAnswers_TenantId_ClinicalRecordId_QuestionKey",
                table: "ClinicalMedicalAnswers",
                columns: new[] { "TenantId", "ClinicalRecordId", "QuestionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalMedicalAnswers_TenantId_PatientId",
                table: "ClinicalMedicalAnswers",
                columns: new[] { "TenantId", "PatientId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalMedicalAnswers");
        }
    }
}
