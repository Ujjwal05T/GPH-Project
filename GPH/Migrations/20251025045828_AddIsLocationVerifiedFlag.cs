using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddIsLocationVerifiedFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocationVerified",
                table: "Shopkeepers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocationVerified",
                table: "Schools",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocationVerified",
                table: "CoachingCenters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocationVerified",
                table: "Shopkeepers");

            migrationBuilder.DropColumn(
                name: "IsLocationVerified",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "IsLocationVerified",
                table: "CoachingCenters");
        }
    }
}
