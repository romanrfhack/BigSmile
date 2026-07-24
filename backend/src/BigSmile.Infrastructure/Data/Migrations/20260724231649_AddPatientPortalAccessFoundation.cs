using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPortalAccessFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientPortalAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LoginName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NormalizedLoginName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastFailedLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastSuccessfulLoginAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SessionVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPortalAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPortalAccounts_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalAccounts_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientPortalInvitations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
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
                    table.PrimaryKey("PK_PatientPortalInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientPortalInvitations_PatientPortalAccounts_ConsumedByPatientPortalAccountId",
                        column: x => x.ConsumedByPatientPortalAccountId,
                        principalTable: "PatientPortalAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalInvitations_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientPortalInvitations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAccounts_PatientId",
                table: "PatientPortalAccounts",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAccounts_TenantId_NormalizedLoginName",
                table: "PatientPortalAccounts",
                columns: new[] { "TenantId", "NormalizedLoginName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalAccounts_TenantId_PatientId",
                table: "PatientPortalAccounts",
                columns: new[] { "TenantId", "PatientId" },
                unique: true,
                filter: "[PatientId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalInvitations_ConsumedByPatientPortalAccountId",
                table: "PatientPortalInvitations",
                column: "ConsumedByPatientPortalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalInvitations_PatientId",
                table: "PatientPortalInvitations",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalInvitations_TenantId_ExpiresAtUtc",
                table: "PatientPortalInvitations",
                columns: new[] { "TenantId", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalInvitations_TenantId_PatientId_CreatedAtUtc",
                table: "PatientPortalInvitations",
                columns: new[] { "TenantId", "PatientId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientPortalInvitations_TokenHash",
                table: "PatientPortalInvitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientPortalInvitations");

            migrationBuilder.DropTable(
                name: "PatientPortalAccounts");
        }
    }
}
