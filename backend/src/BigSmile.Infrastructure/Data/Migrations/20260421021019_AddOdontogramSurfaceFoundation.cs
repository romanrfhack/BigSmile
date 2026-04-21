using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOdontogramSurfaceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OdontogramSurfaceStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdontogramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToothCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    SurfaceCode = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdontogramSurfaceStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OdontogramSurfaceStates_Odontograms_OdontogramId",
                        column: x => x.OdontogramId,
                        principalTable: "Odontograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO [OdontogramSurfaceStates] ([Id], [OdontogramId], [ToothCode], [SurfaceCode], [Status], [UpdatedAtUtc], [UpdatedByUserId])
                SELECT
                    NEWID(),
                    [teeth].[OdontogramId],
                    [teeth].[ToothCode],
                    [surfaces].[SurfaceCode],
                    N'Unknown',
                    [odontograms].[CreatedAtUtc],
                    [odontograms].[CreatedByUserId]
                FROM [OdontogramToothStates] AS [teeth]
                INNER JOIN [Odontograms] AS [odontograms]
                    ON [odontograms].[Id] = [teeth].[OdontogramId]
                CROSS JOIN (
                    VALUES (N'O'), (N'M'), (N'D'), (N'B'), (N'L')
                ) AS [surfaces]([SurfaceCode]);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_OdontogramSurfaceStates_OdontogramId_ToothCode_SurfaceCode",
                table: "OdontogramSurfaceStates",
                columns: new[] { "OdontogramId", "ToothCode", "SurfaceCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OdontogramSurfaceStates");
        }
    }
}
