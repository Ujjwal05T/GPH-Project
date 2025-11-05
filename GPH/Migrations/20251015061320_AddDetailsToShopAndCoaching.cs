using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailsToShopAndCoaching : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Shopkeepers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Shopkeepers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Classes",
                table: "CoachingCenters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "CoachingCenters",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentCount",
                table: "CoachingCenters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subjects",
                table: "CoachingCenters",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherName",
                table: "CoachingCenters",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Shopkeepers");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Shopkeepers");

            migrationBuilder.DropColumn(
                name: "Classes",
                table: "CoachingCenters");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "CoachingCenters");

            migrationBuilder.DropColumn(
                name: "StudentCount",
                table: "CoachingCenters");

            migrationBuilder.DropColumn(
                name: "Subjects",
                table: "CoachingCenters");

            migrationBuilder.DropColumn(
                name: "TeacherName",
                table: "CoachingCenters");
        }
    }
}
