using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BigSmile.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixUserBranchAssignmentDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBranchAssignments_Branches_BranchId",
                table: "UserBranchAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBranchAssignments_Branches_BranchId",
                table: "UserBranchAssignments",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBranchAssignments_Branches_BranchId",
                table: "UserBranchAssignments");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBranchAssignments_Branches_BranchId",
                table: "UserBranchAssignments",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
