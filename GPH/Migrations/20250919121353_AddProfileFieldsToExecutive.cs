using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileFieldsToExecutive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "SalesExecutives",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedArea",
                table: "SalesExecutives",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "SalesExecutives",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "SalesExecutives",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "AssignedArea",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "SalesExecutives");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "SalesExecutives");
        }
    }
}
