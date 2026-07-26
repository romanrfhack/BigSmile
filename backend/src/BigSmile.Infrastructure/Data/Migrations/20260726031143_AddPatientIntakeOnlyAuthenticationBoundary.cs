using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIntakeOnlyAuthenticationBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientIntakeAuthenticationAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientPortalAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientIntakeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientIntakeAccessLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientIntakeAuthenticationAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAuthenticationAuditEntries_PatientIntakeAccessLinks_PatientIntakeAccessLinkId",
                        column: x => x.PatientIntakeAccessLinkId,
                        principalTable: "PatientIntakeAccessLinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAuthenticationAuditEntries_PatientIntakes_PatientIntakeId",
                        column: x => x.PatientIntakeId,
                        principalTable: "PatientIntakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAuthenticationAuditEntries_PatientPortalAccounts_PatientPortalAccountId",
                        column: x => x.PatientPortalAccountId,
                        principalTable: "PatientPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientIntakeAuthenticationAuditEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAuthenticationAuditEntries_PatientIntakeAccessLinkId",
                table: "PatientIntakeAuthenticationAuditEntries",
                column: "PatientIntakeAccessLinkId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAuthenticationAuditEntries_PatientIntakeId",
                table: "PatientIntakeAuthenticationAuditEntries",
                column: "PatientIntakeId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAuthenticationAuditEntries_PatientPortalAccountId",
                table: "PatientIntakeAuthenticationAuditEntries",
                column: "PatientPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAuthenticationAuditEntries_TenantId_PatientIntakeId_OccurredAtUtc",
                table: "PatientIntakeAuthenticationAuditEntries",
                columns: new[] { "TenantId", "PatientIntakeId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientIntakeAuthenticationAuditEntries_TenantId_PatientPortalAccountId_OccurredAtUtc",
                table: "PatientIntakeAuthenticationAuditEntries",
                columns: new[] { "TenantId", "PatientPortalAccountId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientIntakeAuthenticationAuditEntries");
        }
    }
}
