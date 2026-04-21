using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOdontogramSurfaceFindingHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OdontogramSurfaceFindingHistoryEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdontogramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToothCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SurfaceCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    FindingType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    EntryType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceFindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdontogramSurfaceFindingHistoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdontogramSurfaceFindingHistoryEntries_Odontograms_OdontogramId",
                        column: x => x.OdontogramId,
                        principalTable: "Odontograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OdontogramSurfaceFindingHistoryEntries_OdontogramId_ToothCode_SurfaceCode_ChangedAtUtc",
                table: "OdontogramSurfaceFindingHistoryEntries",
                columns: new[] { "OdontogramId", "ToothCode", "SurfaceCode", "ChangedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OdontogramSurfaceFindingHistoryEntries");
        }
    }
}
