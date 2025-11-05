using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class CreateRolesTableAndLinkToExecutive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "SalesExecutives",
                newName: "RoleId");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Executive" },
                    { 2, "ASM" },
                    { 3, "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesExecutives_RoleId",
                table: "SalesExecutives",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesExecutives_Roles_RoleId",
                table: "SalesExecutives",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesExecutives_Roles_RoleId",
                table: "SalesExecutives");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_SalesExecutives_RoleId",
                table: "SalesExecutives");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                table: "SalesExecutives",
                newName: "Role");
        }
    }
}
