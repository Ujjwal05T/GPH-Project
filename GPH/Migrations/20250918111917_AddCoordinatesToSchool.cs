using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddCoordinatesToSchool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "OfficialLatitude",
                table: "Schools",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "OfficialLongitude",
                table: "Schools",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfficialLatitude",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "OfficialLongitude",
                table: "Schools");
        }
    }
}
