using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerHierarchyToExecutive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "SalesExecutives",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesExecutives_ManagerId",
                table: "SalesExecutives",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesExecutives_SalesExecutives_ManagerId",
                table: "SalesExecutives",
                column: "ManagerId",
                principalTable: "SalesExecutives",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesExecutives_SalesExecutives_ManagerId",
                table: "SalesExecutives");

            migrationBuilder.DropIndex(
                name: "IX_SalesExecutives_ManagerId",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "SalesExecutives");
        }
    }
}
