using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPortalAuthenticationBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientPortalAuthenticationAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientPortalAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientPortalInvitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    ActorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPortalAuthenticationAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPortalAuthenticationAuditEntries_PatientPortalAccounts_PatientPortalAccountId",
                        column: x => x.PatientPortalAccountId,
                        principalTable: "PatientPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalAuthenticationAuditEntries_PatientPortalInvitations_PatientPortalInvitationId",
                        column: x => x.PatientPortalInvitationId,
                        principalTable: "PatientPortalInvitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalAuthenticationAuditEntries_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalAuthenticationAuditEntries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAuthenticationAuditEntries_PatientId",
                table: "PatientPortalAuthenticationAuditEntries",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAuthenticationAuditEntries_PatientPortalAccountId",
                table: "PatientPortalAuthenticationAuditEntries",
                column: "PatientPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAuthenticationAuditEntries_PatientPortalInvitationId",
                table: "PatientPortalAuthenticationAuditEntries",
                column: "PatientPortalInvitationId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAuthenticationAuditEntries_TenantId_PatientId_OccurredAtUtc",
                table: "PatientPortalAuthenticationAuditEntries",
                columns: new[] { "TenantId", "PatientId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAuthenticationAuditEntries_TenantId_PatientPortalAccountId_OccurredAtUtc",
                table: "PatientPortalAuthenticationAuditEntries",
                columns: new[] { "TenantId", "PatientPortalAccountId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientPortalAuthenticationAuditEntries");
        }
    }
}
