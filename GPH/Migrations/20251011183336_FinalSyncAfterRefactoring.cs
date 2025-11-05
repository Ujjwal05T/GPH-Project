using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class FinalSyncAfterRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesExecutives_Roles_RoleId",
                table: "SalesExecutives");

            migrationBuilder.DropIndex(
                name: "IX_SalesExecutives_Username",
                table: "SalesExecutives");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesExecutives_Roles_RoleId",
                table: "SalesExecutives",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesExecutives_Roles_RoleId",
                table: "SalesExecutives");

            migrationBuilder.CreateIndex(
                name: "IX_SalesExecutives_Username",
                table: "SalesExecutives",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesExecutives_Roles_RoleId",
                table: "SalesExecutives",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
