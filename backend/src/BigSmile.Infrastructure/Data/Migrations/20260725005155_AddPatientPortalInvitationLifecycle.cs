using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPortalInvitationLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientPortalSecurityAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientPortalInvitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPortalSecurityAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPortalSecurityAuditEntries_PatientPortalInvitations_PatientPortalInvitationId",
                        column: x => x.PatientPortalInvitationId,
                        principalTable: "PatientPortalInvitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalSecurityAuditEntries_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalSecurityAuditEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalInvitations_TenantId_PatientId_Purpose",
                table: "PatientPortalInvitations",
                columns: new[] { "TenantId", "PatientId", "Purpose" },
                unique: true,
                filter: "[RevokedAtUtc] IS NULL AND [ConsumedAtUtc] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalSecurityAuditEntries_PatientId",
                table: "PatientPortalSecurityAuditEntries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalSecurityAuditEntries_PatientPortalInvitationId",
                table: "PatientPortalSecurityAuditEntries",
                column: "PatientPortalInvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalSecurityAuditEntries_TenantId_PatientId_OccurredAtUtc",
                table: "PatientPortalSecurityAuditEntries",
                columns: new[] { "TenantId", "PatientId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalSecurityAuditEntries_TenantId_PatientPortalInvitationId_OccurredAtUtc",
                table: "PatientPortalSecurityAuditEntries",
                columns: new[] { "TenantId", "PatientPortalInvitationId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientPortalSecurityAuditEntries");

            migrationBuilder.DropIndex(
                name: "IX_PatientPortalInvitations_TenantId_PatientId_Purpose",
                table: "PatientPortalInvitations");
        }
    }
}
