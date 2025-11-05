using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class SeparateLocationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_Schools_SchoolId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_SchoolId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "SchoolId",
                table: "Visits",
                newName: "LocationType");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Visits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "BeatPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LocationType",
                table: "BeatPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CoachingCenters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachingCenters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shopkeepers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shopkeepers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoachingCenters");

            migrationBuilder.DropTable(
                name: "Shopkeepers");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "BeatPlans");

            migrationBuilder.DropColumn(
                name: "LocationType",
                table: "BeatPlans");

            migrationBuilder.RenameColumn(
                name: "LocationType",
                table: "Visits",
                newName: "SchoolId");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Schools",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SchoolId",
                table: "Visits",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_Schools_SchoolId",
                table: "Visits",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
