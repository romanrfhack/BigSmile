using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIntakeDraftFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientIntakes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientPortalAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Origin = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Sex = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MaritalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferredBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreferredPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    HomePhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    WorkPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ResponsiblePartyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsiblePartyRelationship = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ResponsiblePartyPhone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ReasonForVisit = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CanonicalPatientBaselineJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanonicalPatientBaselineCapturedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentRevisionNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastEffectiveSavedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakes", x => x.Id);
                    table.CheckConstraint("CK_PatientIntakes_CurrentRevisionNumber", "[CurrentRevisionNumber] >= 0");
                    table.CheckConstraint("CK_PatientIntakes_ExpiryOrder", "[ExpiresAtUtc] > [CreatedAtUtc]");
                    table.CheckConstraint("CK_PatientIntakes_OriginPatientLink", "(([Origin] = N'ExistingPatientPortal' AND [PatientId] IS NOT NULL AND [CanonicalPatientBaselineJson] IS NOT NULL AND [CanonicalPatientBaselineCapturedAtUtc] IS NOT NULL) OR ([Origin] = N'NewPatientWaitingRoom' AND [PatientId] IS NULL AND [CanonicalPatientBaselineJson] IS NULL AND [CanonicalPatientBaselineCapturedAtUtc] IS NULL))");
                    table.ForeignKey(
                        name: "FK_PatientIntakes_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakes_PatientPortalAccounts_PatientPortalAccountId",
                        column: x => x.PatientPortalAccountId,
                        principalTable: "PatientPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientIntakeMedicalAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientIntakeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakeMedicalAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientIntakeMedicalAnswers_PatientIntakes_PatientIntakeId",
                        column: x => x.PatientIntakeId,
                        principalTable: "PatientIntakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientIntakeMedicalAnswers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PatientIntakeRevisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientIntakeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActorPatientPortalAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedFieldsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SnapshotJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakeRevisions", x => x.Id);
                    table.CheckConstraint("CK_PatientIntakeRevisions_RevisionNumber", "[RevisionNumber] > 0");
                    table.ForeignKey(
                        name: "FK_PatientIntakeRevisions_PatientIntakes_PatientIntakeId",
                        column: x => x.PatientIntakeId,
                        principalTable: "PatientIntakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeRevisions_PatientPortalAccounts_ActorPatientPortalAccountId",
                        column: x => x.ActorPatientPortalAccountId,
                        principalTable: "PatientPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeRevisions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeMedicalAnswers_PatientIntakeId",
                table: "PatientIntakeMedicalAnswers",
                column: "PatientIntakeId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeMedicalAnswers_TenantId_PatientIntakeId_QuestionKey",
                table: "PatientIntakeMedicalAnswers",
                columns: new[] { "TenantId", "PatientIntakeId", "QuestionKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeRevisions_ActorPatientPortalAccountId",
                table: "PatientIntakeRevisions",
                column: "ActorPatientPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeRevisions_PatientIntakeId",
                table: "PatientIntakeRevisions",
                column: "PatientIntakeId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeRevisions_TenantId_ActorPatientPortalAccountId_OccurredAtUtc",
                table: "PatientIntakeRevisions",
                columns: new[] { "TenantId", "ActorPatientPortalAccountId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeRevisions_TenantId_PatientIntakeId_RevisionNumber",
                table: "PatientIntakeRevisions",
                columns: new[] { "TenantId", "PatientIntakeId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_BranchId",
                table: "PatientIntakes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_PatientId",
                table: "PatientIntakes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_PatientPortalAccountId",
                table: "PatientIntakes",
                column: "PatientPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_TenantId_PatientId",
                table: "PatientIntakes",
                columns: new[] { "TenantId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_TenantId_PatientPortalAccountId",
                table: "PatientIntakes",
                columns: new[] { "TenantId", "PatientPortalAccountId" },
                unique: true,
                filter: "[Status] = N'Draft'");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakes_TenantId_Status_ExpiresAtUtc",
                table: "PatientIntakes",
                columns: new[] { "TenantId", "Status", "ExpiresAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientIntakeMedicalAnswers");

            migrationBuilder.DropTable(
                name: "PatientIntakeRevisions");

            migrationBuilder.DropTable(
                name: "PatientIntakes");
        }
    }
}
