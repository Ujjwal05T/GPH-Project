using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPH.Migrations
{
    /// <inheritdoc />
    public partial class CreateBeatAssignmentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonthlyBeatAssignments");

            migrationBuilder.CreateTable(
                name: "BeatAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    AssignedMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeatAssignments_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeatAssignments_SalesExecutiveId",
                table: "BeatAssignments",
                column: "SalesExecutiveId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatAssignments");

            migrationBuilder.CreateTable(
                name: "MonthlyBeatAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesExecutiveId = table.Column<int>(type: "int", nullable: false),
                    Area = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyBeatAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyBeatAssignments_SalesExecutives_SalesExecutiveId",
                        column: x => x.SalesExecutiveId,
                        principalTable: "SalesExecutives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyBeatAssignments_SalesExecutiveId",
                table: "MonthlyBeatAssignments",
                column: "SalesExecutiveId");
        }
    }
}
