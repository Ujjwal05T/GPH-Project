using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TeacherId",
                table: "Orders",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Teachers_TeacherId",
                table: "Orders",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Teachers_TeacherId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TeacherId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Orders");
        }
    }
}
