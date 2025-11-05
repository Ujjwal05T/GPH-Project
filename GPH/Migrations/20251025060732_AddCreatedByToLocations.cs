using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByExecutiveId",
                table: "Shopkeepers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByExecutiveId",
                table: "Schools",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByExecutiveId",
                table: "CoachingCenters",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByExecutiveId",
                table: "Shopkeepers");

            migrationBuilder.DropColumn(
                name: "CreatedByExecutiveId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "CreatedByExecutiveId",
                table: "CoachingCenters");
        }
    }
}
