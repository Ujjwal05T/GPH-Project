using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyTrackings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalDistanceKm = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyTrackings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyTrackings_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DailyTrackingId = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationPoints_DailyTrackings_DailyTrackingId",
                        column: x => x.DailyTrackingId,
                        principalTable: "DailyTrackings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyTrackings_SalesExecutiveId",
                table: "DailyTrackings",
                column: "SalesExecutiveId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationPoints_DailyTrackingId",
                table: "LocationPoints",
                column: "DailyTrackingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationPoints");

            migrationBuilder.DropTable(
                name: "DailyTrackings");
        }
    }
}
