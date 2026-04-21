using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOdontogramSurfaceFindingsFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OdontogramSurfaceFindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdontogramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToothCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SurfaceCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    FindingType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdontogramSurfaceFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdontogramSurfaceFindings_Odontograms_OdontogramId",
                        column: x => x.OdontogramId,
                        principalTable: "Odontograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OdontogramSurfaceFindings_OdontogramId_ToothCode_SurfaceCode",
                table: "OdontogramSurfaceFindings",
                columns: new[] { "OdontogramId", "ToothCode", "SurfaceCode" });

            migrationBuilder.CreateIndex(
                name: "IX_OdontogramSurfaceFindings_OdontogramId_ToothCode_SurfaceCode_FindingType",
                table: "OdontogramSurfaceFindings",
                columns: new[] { "OdontogramId", "ToothCode", "SurfaceCode", "FindingType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OdontogramSurfaceFindings");
        }
    }
}
