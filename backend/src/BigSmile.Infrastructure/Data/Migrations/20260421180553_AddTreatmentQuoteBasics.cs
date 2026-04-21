using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentQuoteBasics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreatmentQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LastUpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentQuotes_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentQuotes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TreatmentQuotes_TreatmentPlans_TreatmentPlanId",
                        column: x => x.TreatmentPlanId,
                        principalTable: "TreatmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentQuoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TreatmentQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTreatmentPlanItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ToothCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    SurfaceCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentQuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentQuoteItems_TreatmentQuotes_TreatmentQuoteId",
                        column: x => x.TreatmentQuoteId,
                        principalTable: "TreatmentQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentQuoteItems_TreatmentQuoteId",
                table: "TreatmentQuoteItems",
                column: "TreatmentQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentQuotes_PatientId",
                table: "TreatmentQuotes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentQuotes_TenantId",
                table: "TreatmentQuotes",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentQuotes_TreatmentPlanId",
                table: "TreatmentQuotes",
                column: "TreatmentPlanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TreatmentQuoteItems");

            migrationBuilder.DropTable(
                name: "TreatmentQuotes");
        }
    }
}
