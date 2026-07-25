using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIntakeAccessLinkFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientIntakeAccessLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Purpose = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConsumedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsumedByPatientPortalAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakeAccessLinks", x => x.Id);
                    table.CheckConstraint("CK_PatientIntakeAccessLinks_ConsumptionMetadata", "(([ConsumedAtUtc] IS NULL AND [ConsumedByPatientPortalAccountId] IS NULL) OR ([ConsumedAtUtc] IS NOT NULL AND [ConsumedByPatientPortalAccountId] IS NOT NULL))");
                    table.CheckConstraint("CK_PatientIntakeAccessLinks_ExpiryOrder", "[ExpiresAtUtc] > [CreatedAtUtc]");
                    table.CheckConstraint("CK_PatientIntakeAccessLinks_RevocationMetadata", "(([RevokedAtUtc] IS NULL AND [RevokedByUserId] IS NULL) OR ([RevokedAtUtc] IS NOT NULL AND [RevokedByUserId] IS NOT NULL))");
                    table.CheckConstraint("CK_PatientIntakeAccessLinks_SingleResolution", "NOT ([RevokedAtUtc] IS NOT NULL AND [ConsumedAtUtc] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_PatientIntakeAccessLinks_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAccessLinks_PatientPortalAccounts_ConsumedByPatientPortalAccountId",
                        column: x => x.ConsumedByPatientPortalAccountId,
                        principalTable: "PatientPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAccessLinks_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientIntakeAccessLinkAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PatientIntakeAccessLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakeAccessLinkAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAccessLinkAuditEntries_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAccessLinkAuditEntries_PatientIntakeAccessLinks_PatientIntakeAccessLinkId",
                        column: x => x.PatientIntakeAccessLinkId,
                        principalTable: "PatientIntakeAccessLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAccessLinkAuditEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinkAuditEntries_BranchId",
                table: "PatientIntakeAccessLinkAuditEntries",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinkAuditEntries_PatientIntakeAccessLinkId",
                table: "PatientIntakeAccessLinkAuditEntries",
                column: "PatientIntakeAccessLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinkAuditEntries_TenantId_ActorId_OccurredAtUtc",
                table: "PatientIntakeAccessLinkAuditEntries",
                columns: new[] { "TenantId", "ActorId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinkAuditEntries_TenantId_PatientIntakeAccessLinkId_OccurredAtUtc",
                table: "PatientIntakeAccessLinkAuditEntries",
                columns: new[] { "TenantId", "PatientIntakeAccessLinkId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinks_BranchId",
                table: "PatientIntakeAccessLinks",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinks_ConsumedByPatientPortalAccountId",
                table: "PatientIntakeAccessLinks",
                column: "ConsumedByPatientPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinks_TenantId_BranchId_CreatedAtUtc",
                table: "PatientIntakeAccessLinks",
                columns: new[] { "TenantId", "BranchId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinks_TenantId_CreatedAtUtc",
                table: "PatientIntakeAccessLinks",
                columns: new[] { "TenantId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinks_TenantId_ExpiresAtUtc",
                table: "PatientIntakeAccessLinks",
                columns: new[] { "TenantId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAccessLinks_TokenHash",
                table: "PatientIntakeAccessLinks",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientIntakeAccessLinkAuditEntries");

            migrationBuilder.DropTable(
                name: "PatientIntakeAccessLinks");
        }
    }
}
